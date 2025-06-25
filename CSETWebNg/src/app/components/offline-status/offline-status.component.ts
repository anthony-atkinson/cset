import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { OfflineService, OfflineStatus } from '../../services/offline.service';

@Component({
  selector: 'app-offline-status',
  templateUrl: './offline-status.component.html',
  styleUrls: ['./offline-status.component.scss']
})
export class OfflineStatusComponent implements OnInit, OnDestroy {
  offlineStatus: OfflineStatus = {
    isOnline: true,
    hasPendingSync: false,
    pendingItemsCount: 0,
    syncInProgress: false
  };

  private statusSubscription: Subscription;

  constructor(private offlineService: OfflineService) {}

  ngOnInit(): void {
    this.statusSubscription = this.offlineService.offlineStatus$.subscribe(
      status => {
        this.offlineStatus = status;
      }
    );
  }

  ngOnDestroy(): void {
    if (this.statusSubscription) {
      this.statusSubscription.unsubscribe();
    }
  }

  /**
   * Get the status icon class based on current status
   */
  getStatusIconClass(): string {
    if (!this.offlineStatus.isOnline) {
      return 'offline-icon';
    }
    if (this.offlineStatus.syncInProgress) {
      return 'syncing-icon';
    }
    if (this.offlineStatus.hasPendingSync) {
      return 'pending-sync-icon';
    }
    return 'online-icon';
  }

  /**
   * Get the status text based on current status
   */
  getStatusText(): string {
    if (!this.offlineStatus.isOnline) {
      return 'Offline';
    }
    if (this.offlineStatus.syncInProgress) {
      return 'Syncing...';
    }
    if (this.offlineStatus.hasPendingSync) {
      return `${this.offlineStatus.pendingItemsCount} pending`;
    }
    return 'Online';
  }

  /**
   * Get the status color class
   */
  getStatusColorClass(): string {
    if (!this.offlineStatus.isOnline) {
      return 'status-offline';
    }
    if (this.offlineStatus.syncInProgress) {
      return 'status-syncing';
    }
    if (this.offlineStatus.hasPendingSync) {
      return 'status-pending';
    }
    return 'status-online';
  }

  /**
   * Manually trigger sync
   */
  triggerSync(): void {
    if (this.offlineStatus.isOnline && this.offlineStatus.hasPendingSync) {
      this.offlineService.attemptSync();
    }
  }

  /**
   * Check if sync button should be shown
   */
  showSyncButton(): boolean {
    return this.offlineStatus.isOnline && 
           this.offlineStatus.hasPendingSync && 
           !this.offlineStatus.syncInProgress;
  }
} 