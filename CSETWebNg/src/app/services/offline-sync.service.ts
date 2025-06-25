import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { OfflineService, OfflineDataItem } from './offline.service';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})
export class OfflineSyncService {
  constructor(
    private http: HttpClient,
    private offlineService: OfflineService,
    private configService: ConfigService
  ) {}

  /**
   * Sync assessment data when online
   */
  syncAssessment(assessmentId: number, data: any): Observable<any> {
    if (!this.offlineService.isOnline()) {
      // Queue for offline sync
      this.offlineService.addToOfflineQueue({
        type: 'assessment',
        action: 'update',
        data: { assessmentId, ...data }
      });
      return of({ success: true, queued: true });
    }

    return this.http.put(`${this.configService.apiUrl}assessment/${assessmentId}`, data)
      .pipe(
        tap(() => {
          // Store updated data locally for offline access
          this.offlineService.storeOfflineData(`assessment_${assessmentId}`, data);
        }),
        catchError(error => {
          // If API call fails, queue for retry
          this.offlineService.addToOfflineQueue({
            type: 'assessment',
            action: 'update',
            data: { assessmentId, ...data }
          });
          return throwError(() => error);
        })
      );
  }

  /**
   * Sync question answers when online
   */
  syncQuestionAnswer(questionId: number, answer: any): Observable<any> {
    if (!this.offlineService.isOnline()) {
      // Queue for offline sync
      this.offlineService.addToOfflineQueue({
        type: 'question',
        action: 'update',
        data: { questionId, answer }
      });
      return of({ success: true, queued: true });
    }

    return this.http.post(`${this.configService.apiUrl}questions/answer`, {
      questionId,
      answer
    }).pipe(
      tap(() => {
        // Store answer locally for offline access
        this.offlineService.storeOfflineData(`question_${questionId}`, answer);
      }),
      catchError(error => {
        // If API call fails, queue for retry
        this.offlineService.addToOfflineQueue({
          type: 'question',
          action: 'update',
          data: { questionId, answer }
        });
        return throwError(() => error);
      })
    );
  }

  /**
   * Sync observation data when online
   */
  syncObservation(observationId: number, data: any): Observable<any> {
    if (!this.offlineService.isOnline()) {
      // Queue for offline sync
      this.offlineService.addToOfflineQueue({
        type: 'observation',
        action: 'update',
        data: { observationId, ...data }
      });
      return of({ success: true, queued: true });
    }

    return this.http.put(`${this.configService.apiUrl}observations/${observationId}`, data)
      .pipe(
        tap(() => {
          // Store observation locally for offline access
          this.offlineService.storeOfflineData(`observation_${observationId}`, data);
        }),
        catchError(error => {
          // If API call fails, queue for retry
          this.offlineService.addToOfflineQueue({
            type: 'observation',
            action: 'update',
            data: { observationId, ...data }
          });
          return throwError(() => error);
        })
      );
  }

  /**
   * Get cached data for offline access
   */
  getCachedData(key: string): any {
    return this.offlineService.getOfflineData(key);
  }

  /**
   * Store data for offline access
   */
  cacheData(key: string, data: any): void {
    this.offlineService.storeOfflineData(key, data);
  }

  /**
   * Check if data is available offline
   */
  isDataAvailableOffline(key: string): boolean {
    return this.offlineService.getOfflineData(key) !== null;
  }

  /**
   * Get offline queue statistics
   */
  getOfflineQueueStats() {
    return this.offlineService.getOfflineQueueStats();
  }

  /**
   * Manually trigger sync
   */
  triggerSync(): Promise<void> {
    return this.offlineService.attemptSync();
  }

  /**
   * Clear offline cache
   */
  clearOfflineCache(): void {
    this.offlineService.clearOfflineData();
  }

  /**
   * Get offline status
   */
  getOfflineStatus() {
    return this.offlineService.getOfflineStatus();
  }
} 