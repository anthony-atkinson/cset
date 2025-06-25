////////////////////////////////
//
//   Copyright 2025 Battelle Energy Alliance, LLC
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
//
////////////////////////////////
import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { ConfigService } from './config.service';
import { HttpClient } from '@angular/common/http';

export interface UserPresence {
  userId: string;
  displayName: string;
  assessmentId: number;
  connectedAt: Date;
  lastActivity: Date;
  isOnline: boolean;
}

export interface AssessmentUpdate {
  assessmentId: number;
  updateType: string;
  updateData: any;
  userId: string;
  timestamp: Date;
}

export interface CollaborativeComment {
  assessmentId: number;
  userId: string;
  comment: any;
  timestamp: Date;
}

export interface EditingIndicator {
  assessmentId: number;
  userId: string;
  editingInfo: any;
  timestamp: Date;
}

export interface ConflictResolution {
  assessmentId: number;
  resolvedBy: string;
  conflictData: any;
  timestamp: Date;
}

export interface CollaborationStatistics {
  assessmentId: number;
  totalActivities: number;
  uniqueUsers: number;
  lastActivity: Date;
  mostActiveUser: string | null;
  conflictCount: number;
}

export interface CollaborationActivity {
  id: number;
  assessmentId: number;
  userId: string;
  activityType: string;
  activityData: string;
  timestamp: Date;
}

export enum ConflictResolutionStrategy {
  KeepLatest = 'KeepLatest',
  KeepEarliest = 'KeepEarliest',
  KeepMostComplete = 'KeepMostComplete',
  Manual = 'Manual'
}

export enum CollaborationPermission {
  View = 'View',
  Edit = 'Edit',
  Comment = 'Comment',
  ResolveConflicts = 'ResolveConflicts',
  ViewHistory = 'ViewHistory'
}

@Injectable({
  providedIn: 'root'
})
export class CollaborationService implements OnDestroy {
  private hubConnection: HubConnection | null = null;
  private connectionState = new BehaviorSubject<boolean>(false);
  private activeUsers = new BehaviorSubject<UserPresence[]>([]);
  private assessmentUpdates = new Subject<AssessmentUpdate>();
  private comments = new Subject<CollaborativeComment>();
  private editingIndicators = new Subject<EditingIndicator>();
  private conflictResolutions = new Subject<ConflictResolution>();
  private userJoined = new Subject<UserPresence>();
  private userLeft = new Subject<UserPresence>();
  private userDisconnected = new Subject<UserPresence>();
  private currentAssessmentId: number | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectInterval = 5000; // 5 seconds

  constructor(
    private configSvc: ConfigService,
    private http: HttpClient
  ) {
    this.initializeConnection();
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  /**
   * Initialize SignalR connection
   */
  private initializeConnection(): void {
    const hubUrl = this.configSvc.apiUrl.replace('/api/', '/collaborationHub');
    
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.getAuthToken()
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Reconnect intervals
      .configureLogging(LogLevel.Information)
      .build();

    this.setupEventHandlers();
    this.startConnection();
  }

  /**
   * Setup SignalR event handlers
   */
  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Connection events
    this.hubConnection.onreconnecting(() => {
      console.log('Collaboration: Attempting to reconnect...');
      this.connectionState.next(false);
    });

    this.hubConnection.onreconnected(() => {
      console.log('Collaboration: Reconnected successfully');
      this.connectionState.next(true);
      this.reconnectAttempts = 0;
      
      // Rejoin current assessment if any
      if (this.currentAssessmentId) {
        this.joinAssessment(this.currentAssessmentId, this.getCurrentUserDisplayName());
      }
    });

    this.hubConnection.onclose(() => {
      console.log('Collaboration: Connection closed');
      this.connectionState.next(false);
      this.attemptReconnect();
    });

    // Collaboration events
    this.hubConnection.on('UserJoined', (user: UserPresence) => {
      console.log('User joined:', user);
      this.userJoined.next(user);
      this.updateActiveUsers(user, true);
    });

    this.hubConnection.on('UserLeft', (user: UserPresence) => {
      console.log('User left:', user);
      this.userLeft.next(user);
      this.updateActiveUsers(user, false);
    });

    this.hubConnection.on('UserDisconnected', (user: UserPresence) => {
      console.log('User disconnected:', user);
      this.userDisconnected.next(user);
      this.updateActiveUsers(user, false);
    });

    this.hubConnection.on('AssessmentJoined', (assessmentId: number, users: UserPresence[]) => {
      console.log('Joined assessment:', assessmentId, 'with users:', users);
      this.activeUsers.next(users);
    });

    this.hubConnection.on('AssessmentUpdated', (update: AssessmentUpdate) => {
      console.log('Assessment updated:', update);
      this.assessmentUpdates.next(update);
    });

    this.hubConnection.on('CommentAdded', (comment: CollaborativeComment) => {
      console.log('Comment added:', comment);
      this.comments.next(comment);
    });

    this.hubConnection.on('UserEditing', (indicator: EditingIndicator) => {
      console.log('User editing:', indicator);
      this.editingIndicators.next(indicator);
    });

    this.hubConnection.on('UserStoppedEditing', (userId: string) => {
      console.log('User stopped editing:', userId);
      // Remove editing indicator for this user
      const currentIndicators = this.editingIndicators.asObservable();
      // Implementation would filter out the user's editing indicator
    });

    this.hubConnection.on('ConflictResolved', (resolution: ConflictResolution) => {
      console.log('Conflict resolved:', resolution);
      this.conflictResolutions.next(resolution);
    });
  }

  /**
   * Start SignalR connection
   */
  private async startConnection(): Promise<void> {
    try {
      if (this.hubConnection) {
        await this.hubConnection.start();
        console.log('Collaboration: Connected to SignalR hub');
        this.connectionState.next(true);
        this.reconnectAttempts = 0;
      }
    } catch (error) {
      console.error('Collaboration: Failed to start connection:', error);
      this.connectionState.next(false);
      this.attemptReconnect();
    }
  }

  /**
   * Attempt to reconnect with exponential backoff
   */
  private attemptReconnect(): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      const delay = this.reconnectInterval * Math.pow(2, this.reconnectAttempts - 1);
      
      setTimeout(() => {
        console.log(`Collaboration: Reconnect attempt ${this.reconnectAttempts}`);
        this.startConnection();
      }, delay);
    } else {
      console.error('Collaboration: Max reconnect attempts reached');
    }
  }

  /**
   * Join an assessment for real-time collaboration
   */
  async joinAssessment(assessmentId: number, userDisplayName: string): Promise<void> {
    try {
      if (this.hubConnection && this.connectionState.value) {
        await this.hubConnection.invoke('JoinAssessment', assessmentId, userDisplayName);
        this.currentAssessmentId = assessmentId;
        console.log('Joined assessment:', assessmentId);
      } else {
        console.warn('Cannot join assessment: connection not available');
      }
    } catch (error) {
      console.error('Failed to join assessment:', error);
      throw error;
    }
  }

  /**
   * Leave the current assessment
   */
  async leaveAssessment(): Promise<void> {
    try {
      if (this.hubConnection && this.currentAssessmentId) {
        await this.hubConnection.invoke('LeaveAssessment', this.currentAssessmentId);
        this.currentAssessmentId = null;
        console.log('Left assessment');
      }
    } catch (error) {
      console.error('Failed to leave assessment:', error);
      throw error;
    }
  }

  /**
   * Send assessment update to other users
   */
  async sendAssessmentUpdate(updateType: string, updateData: any): Promise<void> {
    try {
      if (this.hubConnection && this.currentAssessmentId) {
        await this.hubConnection.invoke('SendAssessmentUpdate', this.currentAssessmentId, updateType, updateData);
      }
    } catch (error) {
      console.error('Failed to send assessment update:', error);
      throw error;
    }
  }

  /**
   * Send collaborative comment
   */
  async sendComment(comment: any): Promise<void> {
    try {
      if (this.hubConnection && this.currentAssessmentId) {
        await this.hubConnection.invoke('SendComment', this.currentAssessmentId, comment);
      }
    } catch (error) {
      console.error('Failed to send comment:', error);
      throw error;
    }
  }

  /**
   * Send editing indicator
   */
  async sendEditingIndicator(editingInfo: any): Promise<void> {
    try {
      if (this.hubConnection && this.currentAssessmentId) {
        await this.hubConnection.invoke('SendEditingIndicator', this.currentAssessmentId, editingInfo);
      }
    } catch (error) {
      console.error('Failed to send editing indicator:', error);
      throw error;
    }
  }

  /**
   * Clear editing indicator
   */
  async clearEditingIndicator(): Promise<void> {
    try {
      if (this.hubConnection && this.currentAssessmentId) {
        await this.hubConnection.invoke('ClearEditingIndicator', this.currentAssessmentId);
      }
    } catch (error) {
      console.error('Failed to clear editing indicator:', error);
      throw error;
    }
  }

  /**
   * Resolve conflict
   */
  async resolveConflict(conflictData: any, strategy: ConflictResolutionStrategy): Promise<void> {
    try {
      if (this.hubConnection && this.currentAssessmentId) {
        await this.hubConnection.invoke('ResolveConflict', this.currentAssessmentId, conflictData);
      }
    } catch (error) {
      console.error('Failed to resolve conflict:', error);
      throw error;
    }
  }

  /**
   * Get active users for current assessment
   */
  async getActiveUsers(): Promise<UserPresence[]> {
    try {
      if (this.hubConnection && this.currentAssessmentId) {
        return await this.hubConnection.invoke('GetActiveUsers', this.currentAssessmentId);
      }
      return [];
    } catch (error) {
      console.error('Failed to get active users:', error);
      return [];
    }
  }

  /**
   * Update activity timestamp
   */
  async updateActivity(): Promise<void> {
    try {
      if (this.hubConnection) {
        await this.hubConnection.invoke('UpdateActivity');
      }
    } catch (error) {
      console.error('Failed to update activity:', error);
    }
  }

  /**
   * Disconnect from SignalR
   */
  async disconnect(): Promise<void> {
    try {
      if (this.hubConnection) {
        await this.leaveAssessment();
        await this.hubConnection.stop();
        this.hubConnection = null;
        this.connectionState.next(false);
        console.log('Collaboration: Disconnected from SignalR hub');
      }
    } catch (error) {
      console.error('Error disconnecting:', error);
    }
  }

  /**
   * Get collaboration history from API
   */
  getCollaborationHistory(assessmentId: number, startDate?: Date, endDate?: Date): Observable<any> {
    let url = `${this.configSvc.apiUrl}collaboration/history/${assessmentId}`;
    const params: any = {};
    
    if (startDate) {
      params.startDate = startDate.toISOString();
    }
    if (endDate) {
      params.endDate = endDate.toISOString();
    }

    return this.http.get(url, { params });
  }

  /**
   * Get collaboration statistics from API
   */
  getCollaborationStatistics(assessmentId: number): Observable<CollaborationStatistics> {
    const url = `${this.configSvc.apiUrl}collaboration/statistics/${assessmentId}`;
    return this.http.get<CollaborationStatistics>(url);
  }

  /**
   * Resolve conflict via API
   */
  resolveConflictViaApi(assessmentId: number, conflictData: any, strategy: ConflictResolutionStrategy): Observable<any> {
    const url = `${this.configSvc.apiUrl}collaboration/resolve-conflict/${assessmentId}`;
    const request = { conflictData, resolutionStrategy: strategy };
    return this.http.post(url, request);
  }

  /**
   * Get user permissions from API
   */
  getUserPermissions(assessmentId: number): Observable<any> {
    const url = `${this.configSvc.apiUrl}collaboration/permissions/${assessmentId}`;
    return this.http.get(url);
  }

  /**
   * Record activity via API
   */
  recordActivity(assessmentId: number, activityType: string, activityData: any): Observable<any> {
    const url = `${this.configSvc.apiUrl}collaboration/record-activity/${assessmentId}`;
    const request = { activityType, activityData };
    return this.http.post(url, request);
  }

  // Observable getters for components
  get connectionState$(): Observable<boolean> {
    return this.connectionState.asObservable();
  }

  get activeUsers$(): Observable<UserPresence[]> {
    return this.activeUsers.asObservable();
  }

  get assessmentUpdates$(): Observable<AssessmentUpdate> {
    return this.assessmentUpdates.asObservable();
  }

  get comments$(): Observable<CollaborativeComment> {
    return this.comments.asObservable();
  }

  get editingIndicators$(): Observable<EditingIndicator> {
    return this.editingIndicators.asObservable();
  }

  get conflictResolutions$(): Observable<ConflictResolution> {
    return this.conflictResolutions.asObservable();
  }

  get userJoined$(): Observable<UserPresence> {
    return this.userJoined.asObservable();
  }

  get userLeft$(): Observable<UserPresence> {
    return this.userLeft.asObservable();
  }

  get userDisconnected$(): Observable<UserPresence> {
    return this.userDisconnected.asObservable();
  }

  /**
   * Update active users list
   */
  private updateActiveUsers(user: UserPresence, isJoining: boolean): void {
    const currentUsers = this.activeUsers.value;
    
    if (isJoining) {
      const existingIndex = currentUsers.findIndex(u => u.userId === user.userId);
      if (existingIndex >= 0) {
        currentUsers[existingIndex] = user;
      } else {
        currentUsers.push(user);
      }
    } else {
      const filteredUsers = currentUsers.filter(u => u.userId !== user.userId);
      this.activeUsers.next(filteredUsers);
      return;
    }
    
    this.activeUsers.next([...currentUsers]);
  }

  /**
   * Get authentication token
   */
  private getAuthToken(): string {
    // This would get the JWT token from your authentication service
    // For now, return empty string as placeholder
    return '';
  }

  /**
   * Get current user display name
   */
  private getCurrentUserDisplayName(): string {
    // This would get the current user's display name
    // For now, return a placeholder
    return 'Current User';
  }

  /**
   * Check if currently connected
   */
  isConnected(): boolean {
    return this.connectionState.value;
  }

  /**
   * Get current assessment ID
   */
  getCurrentAssessmentId(): number | null {
    return this.currentAssessmentId;
  }
} 