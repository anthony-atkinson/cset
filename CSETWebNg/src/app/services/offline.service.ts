import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, fromEvent, merge, of } from 'rxjs';
import { map, startWith, switchMap, tap } from 'rxjs/operators';

export interface OfflineDataItem {
  id: string;
  type: 'assessment' | 'question' | 'document' | 'observation';
  action: 'create' | 'update' | 'delete';
  data: any;
  timestamp: number;
  retryCount: number;
}

export interface OfflineStatus {
  isOnline: boolean;
  hasPendingSync: boolean;
  pendingItemsCount: number;
  lastSyncAttempt?: number;
  syncInProgress: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class OfflineService {
  private readonly OFFLINE_QUEUE_KEY = 'cset_offline_queue';
  private readonly OFFLINE_DATA_KEY = 'cset_offline_data';
  private readonly MAX_RETRY_COUNT = 3;

  private onlineStatusSubject = new BehaviorSubject<boolean>(navigator.onLine);
  private offlineStatusSubject = new BehaviorSubject<OfflineStatus>({
    isOnline: navigator.onLine,
    hasPendingSync: false,
    pendingItemsCount: 0,
    syncInProgress: false
  });

  public onlineStatus$ = this.onlineStatusSubject.asObservable();
  public offlineStatus$ = this.offlineStatusSubject.asObservable();

  constructor() {
    this.initializeOfflineDetection();
    this.loadOfflineQueue();
    this.updateOfflineStatus();
  }

  /**
   * Initialize offline detection using browser events
   */
  private initializeOfflineDetection(): void {
    // Listen for online/offline events
    const online$ = fromEvent(window, 'online').pipe(map(() => true));
    const offline$ = fromEvent(window, 'offline').pipe(map(() => false));

    merge(online$, offline$)
      .pipe(
        startWith(navigator.onLine)
      )
      .subscribe(isOnline => {
        this.onlineStatusSubject.next(isOnline);
        this.updateOfflineStatus();
        
        if (isOnline) {
          this.attemptSync();
        }
      });
  }

  /**
   * Check if the application is currently online
   */
  isOnline(): boolean {
    return this.onlineStatusSubject.value;
  }

  /**
   * Get current offline status
   */
  getOfflineStatus(): OfflineStatus {
    return this.offlineStatusSubject.value;
  }

  /**
   * Add data to offline queue for later synchronization
   */
  addToOfflineQueue(item: Omit<OfflineDataItem, 'id' | 'timestamp' | 'retryCount'>): void {
    const offlineItem: OfflineDataItem = {
      ...item,
      id: this.generateId(),
      timestamp: Date.now(),
      retryCount: 0
    };

    const queue = this.getOfflineQueue();
    queue.push(offlineItem);
    this.saveOfflineQueue(queue);
    this.updateOfflineStatus();
  }

  /**
   * Get all items in the offline queue
   */
  getOfflineQueue(): OfflineDataItem[] {
    try {
      const queueData = localStorage.getItem(this.OFFLINE_QUEUE_KEY);
      return queueData ? JSON.parse(queueData) : [];
    } catch (error) {
      console.error('Error reading offline queue:', error);
      return [];
    }
  }

  /**
   * Save offline queue to localStorage
   */
  private saveOfflineQueue(queue: OfflineDataItem[]): void {
    try {
      localStorage.setItem(this.OFFLINE_QUEUE_KEY, JSON.stringify(queue));
    } catch (error) {
      console.error('Error saving offline queue:', error);
    }
  }

  /**
   * Load offline queue from localStorage
   */
  private loadOfflineQueue(): void {
    this.updateOfflineStatus();
  }

  /**
   * Attempt to synchronize offline data when online
   */
  async attemptSync(): Promise<void> {
    if (!this.isOnline()) {
      return;
    }

    const queue = this.getOfflineQueue();
    if (queue.length === 0) {
      this.updateOfflineStatus();
      return;
    }

    const currentStatus = this.offlineStatusSubject.value;
    if (currentStatus.syncInProgress) {
      return;
    }

    // Update status to show sync in progress
    const newStatus = { ...currentStatus, syncInProgress: true };
    this.offlineStatusSubject.next(newStatus);

    try {
      const remainingItems = await this.processQueueItems(queue);
      this.saveOfflineQueue(remainingItems);
      this.updateOfflineStatus();
    } catch (error) {
      console.error('Error during offline sync:', error);
      this.updateOfflineStatus();
    }
  }

  /**
   * Process items in the offline queue
   */
  private async processQueueItems(queue: OfflineDataItem[]): Promise<OfflineDataItem[]> {
    const remainingItems: OfflineDataItem[] = [];

    for (const item of queue) {
      try {
        await this.processQueueItem(item);
      } catch (error) {
        console.error(`Error processing offline item ${item.id}:`, error);
        
        if (item.retryCount < this.MAX_RETRY_COUNT) {
          item.retryCount++;
          remainingItems.push(item);
        } else {
          console.warn(`Max retry count reached for item ${item.id}, removing from queue`);
        }
      }
    }

    return remainingItems;
  }

  /**
   * Process a single offline queue item
   */
  private async processQueueItem(item: OfflineDataItem): Promise<void> {
    // This is a placeholder implementation
    // In a real implementation, you would make actual API calls here
    // based on the item type and action
    
    switch (item.type) {
      case 'assessment':
        await this.syncAssessment(item);
        break;
      case 'question':
        await this.syncQuestion(item);
        break;
      case 'document':
        await this.syncDocument(item);
        break;
      case 'observation':
        await this.syncObservation(item);
        break;
      default:
        throw new Error(`Unknown item type: ${item.type}`);
    }
  }

  /**
   * Sync assessment data
   */
  private async syncAssessment(item: OfflineDataItem): Promise<void> {
    // Placeholder for assessment sync logic
    console.log('Syncing assessment:', item);
    await new Promise(resolve => setTimeout(resolve, 100)); // Simulate API call
  }

  /**
   * Sync question data
   */
  private async syncQuestion(item: OfflineDataItem): Promise<void> {
    // Placeholder for question sync logic
    console.log('Syncing question:', item);
    await new Promise(resolve => setTimeout(resolve, 100)); // Simulate API call
  }

  /**
   * Sync document data
   */
  private async syncDocument(item: OfflineDataItem): Promise<void> {
    // Placeholder for document sync logic
    console.log('Syncing document:', item);
    await new Promise(resolve => setTimeout(resolve, 100)); // Simulate API call
  }

  /**
   * Sync observation data
   */
  private async syncObservation(item: OfflineDataItem): Promise<void> {
    // Placeholder for observation sync logic
    console.log('Syncing observation:', item);
    await new Promise(resolve => setTimeout(resolve, 100)); // Simulate API call
  }

  /**
   * Store data for offline access
   */
  storeOfflineData(key: string, data: any): void {
    try {
      const offlineData = this.getOfflineData();
      offlineData[key] = {
        data,
        timestamp: Date.now()
      };
      localStorage.setItem(this.OFFLINE_DATA_KEY, JSON.stringify(offlineData));
    } catch (error) {
      console.error('Error storing offline data:', error);
    }
  }

  /**
   * Retrieve data stored for offline access
   */
  getOfflineData(key?: string): any {
    try {
      const offlineData = localStorage.getItem(this.OFFLINE_DATA_KEY);
      const data = offlineData ? JSON.parse(offlineData) : {};
      return key ? data[key] : data;
    } catch (error) {
      console.error('Error reading offline data:', error);
      return key ? null : {};
    }
  }

  /**
   * Clear offline data
   */
  clearOfflineData(key?: string): void {
    try {
      if (key) {
        const offlineData = this.getOfflineData();
        delete offlineData[key];
        localStorage.setItem(this.OFFLINE_DATA_KEY, JSON.stringify(offlineData));
      } else {
        localStorage.removeItem(this.OFFLINE_DATA_KEY);
      }
    } catch (error) {
      console.error('Error clearing offline data:', error);
    }
  }

  /**
   * Update offline status
   */
  private updateOfflineStatus(): void {
    const queue = this.getOfflineQueue();
    const currentStatus = this.offlineStatusSubject.value;
    
    const newStatus: OfflineStatus = {
      isOnline: this.isOnline(),
      hasPendingSync: queue.length > 0,
      pendingItemsCount: queue.length,
      lastSyncAttempt: currentStatus.lastSyncAttempt,
      syncInProgress: currentStatus.syncInProgress
    };

    this.offlineStatusSubject.next(newStatus);
  }

  /**
   * Generate unique ID for offline queue items
   */
  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }

  /**
   * Clear the offline queue
   */
  clearOfflineQueue(): void {
    this.saveOfflineQueue([]);
    this.updateOfflineStatus();
  }

  /**
   * Get offline queue statistics
   */
  getOfflineQueueStats(): { total: number; byType: Record<string, number>; byAction: Record<string, number> } {
    const queue = this.getOfflineQueue();
    const byType: Record<string, number> = {};
    const byAction: Record<string, number> = {};

    queue.forEach(item => {
      byType[item.type] = (byType[item.type] || 0) + 1;
      byAction[item.action] = (byAction[item.action] || 0) + 1;
    });

    return {
      total: queue.length,
      byType,
      byAction
    };
  }
} 