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
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { CollaborationService, UserPresence } from '../../../services/collaboration.service';

@Component({
  selector: 'app-user-presence',
  templateUrl: './user-presence.component.html',
  styleUrls: ['./user-presence.component.scss']
})
export class UserPresenceComponent implements OnInit, OnDestroy {
  activeUsers: UserPresence[] = [];
  isConnected = false;
  private subscriptions: Subscription[] = [];

  constructor(private collaborationService: CollaborationService) {}

  ngOnInit(): void {
    // Subscribe to connection state
    this.subscriptions.push(
      this.collaborationService.connectionState$.subscribe(
        connected => this.isConnected = connected
      )
    );

    // Subscribe to active users
    this.subscriptions.push(
      this.collaborationService.activeUsers$.subscribe(
        users => this.activeUsers = users
      )
    );

    // Subscribe to user join events
    this.subscriptions.push(
      this.collaborationService.userJoined$.subscribe(
        user => this.handleUserJoined(user)
      )
    );

    // Subscribe to user leave events
    this.subscriptions.push(
      this.collaborationService.userLeft$.subscribe(
        user => this.handleUserLeft(user)
      )
    );

    // Subscribe to user disconnect events
    this.subscriptions.push(
      this.collaborationService.userDisconnected$.subscribe(
        user => this.handleUserDisconnected(user)
      )
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  /**
   * Handle user joining the assessment
   */
  private handleUserJoined(user: UserPresence): void {
    console.log('User joined:', user.displayName);
    // Could add toast notification here
  }

  /**
   * Handle user leaving the assessment
   */
  private handleUserLeft(user: UserPresence): void {
    console.log('User left:', user.displayName);
    // Could add toast notification here
  }

  /**
   * Handle user disconnecting
   */
  private handleUserDisconnected(user: UserPresence): void {
    console.log('User disconnected:', user.displayName);
    // Could add toast notification here
  }

  /**
   * Get user status color based on activity
   */
  getUserStatusColor(user: UserPresence): string {
    if (!user.isOnline) {
      return 'gray';
    }
    
    const timeSinceActivity = Date.now() - user.lastActivity.getTime();
    const minutesSinceActivity = timeSinceActivity / (1000 * 60);
    
    if (minutesSinceActivity < 1) {
      return 'green'; // Very recent activity
    } else if (minutesSinceActivity < 5) {
      return 'orange'; // Recent activity
    } else {
      return 'red'; // Inactive
    }
  }

  /**
   * Get user status text
   */
  getUserStatusText(user: UserPresence): string {
    if (!user.isOnline) {
      return 'Offline';
    }
    
    const timeSinceActivity = Date.now() - user.lastActivity.getTime();
    const minutesSinceActivity = timeSinceActivity / (1000 * 60);
    
    if (minutesSinceActivity < 1) {
      return 'Active now';
    } else if (minutesSinceActivity < 5) {
      return 'Recently active';
    } else {
      return 'Inactive';
    }
  }

  /**
   * Get time since user connected
   */
  getTimeSinceConnected(user: UserPresence): string {
    const timeSinceConnected = Date.now() - user.connectedAt.getTime();
    const minutes = Math.floor(timeSinceConnected / (1000 * 60));
    const hours = Math.floor(minutes / 60);
    
    if (hours > 0) {
      return `${hours}h ${minutes % 60}m`;
    } else {
      return `${minutes}m`;
    }
  }

  /**
   * Get time since last activity
   */
  getTimeSinceActivity(user: UserPresence): string {
    const timeSinceActivity = Date.now() - user.lastActivity.getTime();
    const minutes = Math.floor(timeSinceActivity / (1000 * 60));
    
    if (minutes < 1) {
      return 'Just now';
    } else if (minutes < 60) {
      return `${minutes}m ago`;
    } else {
      const hours = Math.floor(minutes / 60);
      return `${hours}h ago`;
    }
  }
} 