import { TestBed } from '@angular/core/testing';
import { OfflineService, OfflineDataItem, OfflineStatus } from './offline.service';

describe('OfflineService', () => {
  let service: OfflineService;
  let mockLocalStorage: { [key: string]: string } = {};

  beforeEach(() => {
    // Mock localStorage
    spyOn(localStorage, 'getItem').and.callFake((key: string) => {
      return mockLocalStorage[key] || null;
    });
    
    spyOn(localStorage, 'setItem').and.callFake((key: string, value: string) => {
      mockLocalStorage[key] = value;
    });
    
    spyOn(localStorage, 'removeItem').and.callFake((key: string) => {
      delete mockLocalStorage[key];
    });

    // Mock navigator.onLine as writable
    Object.defineProperty(navigator, 'onLine', {
      writable: true,
      value: true
    });

    TestBed.configureTestingModule({
      providers: [OfflineService]
    });

    service = TestBed.inject(OfflineService);
    mockLocalStorage = {};
  });

  afterEach(() => {
    mockLocalStorage = {};
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Online/Offline Detection', () => {
    it('should initialize with current online status', () => {
      Object.defineProperty(navigator, 'onLine', { writable: true, value: true });
      const newService = new OfflineService();
      expect(newService.isOnline()).toBe(true);
    });

    it('should detect offline status', () => {
      Object.defineProperty(navigator, 'onLine', { writable: true, value: false });
      const newService = new OfflineService();
      expect(newService.isOnline()).toBe(false);
    });

    it('should provide online status observable', (done) => {
      service.onlineStatus$.subscribe(isOnline => {
        expect(isOnline).toBe(true);
        done();
      });
    });

    it('should provide offline status observable', (done) => {
      service.offlineStatus$.subscribe(status => {
        expect(status.isOnline).toBe(true);
        expect(status.hasPendingSync).toBe(false);
        expect(status.pendingItemsCount).toBe(0);
        expect(status.syncInProgress).toBe(false);
        done();
      });
    });
  });

  describe('Offline Queue Management', () => {
    const mockQueueItem: Omit<OfflineDataItem, 'id' | 'timestamp' | 'retryCount'> = {
      type: 'assessment',
      action: 'create',
      data: { assessmentId: 123, name: 'Test Assessment' }
    };

    it('should add item to offline queue', () => {
      service.addToOfflineQueue(mockQueueItem);
      
      const queue = service.getOfflineQueue();
      expect(queue.length).toBe(1);
      expect(queue[0].type).toBe('assessment');
      expect(queue[0].action).toBe('create');
      expect(queue[0].data).toEqual(mockQueueItem.data);
      expect(queue[0].retryCount).toBe(0);
    });

    it('should generate unique IDs for queue items', () => {
      service.addToOfflineQueue(mockQueueItem);
      service.addToOfflineQueue(mockQueueItem);
      
      const queue = service.getOfflineQueue();
      expect(queue.length).toBe(2);
      expect(queue[0].id).not.toBe(queue[1].id);
    });

    it('should save queue to localStorage', () => {
      service.addToOfflineQueue(mockQueueItem);
      
      expect(localStorage.setItem).toHaveBeenCalledWith(
        'cset_offline_queue',
        jasmine.any(String)
      );
    });

    it('should load queue from localStorage', () => {
      const mockQueueData = JSON.stringify([{
        id: 'test-id',
        type: 'assessment',
        action: 'create',
        data: { assessmentId: 123 },
        timestamp: Date.now(),
        retryCount: 0
      }]);
      
      mockLocalStorage['cset_offline_queue'] = mockQueueData;
      
      const queue = service.getOfflineQueue();
      expect(queue.length).toBe(1);
      expect(queue[0].id).toBe('test-id');
    });

    it('should handle localStorage errors gracefully', () => {
      spyOn(console, 'error');
      spyOn(JSON, 'parse').and.throwError('Parse error');
      
      mockLocalStorage['cset_offline_queue'] = 'invalid json';
      
      const queue = service.getOfflineQueue();
      expect(queue).toEqual([]);
      expect(console.error).toHaveBeenCalledWith('Error reading offline queue:', jasmine.any(Error));
    });

    it('should clear offline queue', () => {
      service.addToOfflineQueue(mockQueueItem);
      expect(service.getOfflineQueue().length).toBe(1);
      
      service.clearOfflineQueue();
      expect(service.getOfflineQueue().length).toBe(0);
      expect(localStorage.removeItem).toHaveBeenCalledWith('cset_offline_queue');
    });

    it('should get queue statistics', () => {
      service.addToOfflineQueue({ type: 'assessment', action: 'create', data: {} });
      service.addToOfflineQueue({ type: 'question', action: 'update', data: {} });
      service.addToOfflineQueue({ type: 'assessment', action: 'delete', data: {} });
      
      const stats = service.getOfflineQueueStats();
      expect(stats.total).toBe(3);
      expect(stats.byType.assessment).toBe(2);
      expect(stats.byType.question).toBe(1);
      expect(stats.byAction.create).toBe(1);
      expect(stats.byAction.update).toBe(1);
      expect(stats.byAction.delete).toBe(1);
    });
  });

  describe('Offline Data Storage', () => {
    it('should store offline data', () => {
      const testData = { key: 'value', number: 123 };
      service.storeOfflineData('test-key', testData);
      
      expect(localStorage.setItem).toHaveBeenCalledWith(
        'cset_offline_data_test-key',
        JSON.stringify(testData)
      );
    });

    it('should retrieve offline data', () => {
      const testData = { key: 'value', number: 123 };
      mockLocalStorage['cset_offline_data_test-key'] = JSON.stringify(testData);
      
      const retrieved = service.getOfflineData('test-key');
      expect(retrieved).toEqual(testData);
    });

    it('should retrieve all offline data when no key specified', () => {
      const data1 = { key1: 'value1' };
      const data2 = { key2: 'value2' };
      
      mockLocalStorage['cset_offline_data_key1'] = JSON.stringify(data1);
      mockLocalStorage['cset_offline_data_key2'] = JSON.stringify(data2);
      
      const allData = service.getOfflineData();
      expect(allData).toEqual({
        key1: data1,
        key2: data2
      });
    });

    it('should clear specific offline data', () => {
      service.clearOfflineData('test-key');
      expect(localStorage.removeItem).toHaveBeenCalledWith('cset_offline_data_test-key');
    });

    it('should clear all offline data when no key specified', () => {
      mockLocalStorage['cset_offline_data_key1'] = 'data1';
      mockLocalStorage['cset_offline_data_key2'] = 'data2';
      
      service.clearOfflineData();
      
      expect(localStorage.removeItem).toHaveBeenCalledWith('cset_offline_data_key1');
      expect(localStorage.removeItem).toHaveBeenCalledWith('cset_offline_data_key2');
    });

    it('should handle localStorage errors in data operations', () => {
      spyOn(console, 'error');
      spyOn(JSON, 'parse').and.throwError('Parse error');
      
      mockLocalStorage['cset_offline_data_test-key'] = 'invalid json';
      
      const result = service.getOfflineData('test-key');
      expect(result).toBeNull();
      expect(console.error).toHaveBeenCalledWith('Error reading offline data:', jasmine.any(Error));
    });
  });

  describe('Synchronization', () => {
    beforeEach(() => {
      Object.defineProperty(navigator, 'onLine', { writable: true, value: true });
    });

    it('should not sync when offline', async () => {
      Object.defineProperty(navigator, 'onLine', { writable: true, value: false });
      const newService = new OfflineService();
      
      await newService.attemptSync();
      
      // Should not process any items when offline
      expect(newService.getOfflineQueue().length).toBe(0);
    });

    it('should not sync when queue is empty', async () => {
      await service.attemptSync();
      
      // Should not process anything when queue is empty
      expect(service.getOfflineQueue().length).toBe(0);
    });

    it('should not sync when already in progress', async () => {
      service.addToOfflineQueue({
        type: 'assessment',
        action: 'create',
        data: {}
      });
      
      // Start first sync
      const firstSync = service.attemptSync();
      
      // Try to start second sync
      const secondSync = service.attemptSync();
      
      await Promise.all([firstSync, secondSync]);
      
      // Should only process items once
      expect(service.getOfflineQueue().length).toBe(0);
    });

    it('should retry failed items up to max retry count', async () => {
      service.addToOfflineQueue({
        type: 'assessment',
        action: 'create',
        data: {}
      });
      
      // Mock processQueueItem to always fail
      spyOn<any>(service, 'processQueueItem').and.rejectWith(new Error('Network error'));
      
      await service.attemptSync();
      
      const queue = service.getOfflineQueue();
      expect(queue.length).toBe(1);
      expect(queue[0].retryCount).toBe(1);
    });

    it('should remove items after max retry count', async () => {
      service.addToOfflineQueue({
        type: 'assessment',
        action: 'create',
        data: {}
      });
      
      // Mock processQueueItem to always fail
      spyOn<any>(service, 'processQueueItem').and.rejectWith(new Error('Network error'));
      
      // Attempt sync multiple times to reach max retry count
      for (let i = 0; i < 4; i++) {
        await service.attemptSync();
      }
      
      const queue = service.getOfflineQueue();
      expect(queue.length).toBe(0); // Should be removed after max retries
    });
  });

  describe('Status Updates', () => {
    it('should update offline status when queue changes', () => {
      service.addToOfflineQueue({
        type: 'assessment',
        action: 'create',
        data: {}
      });
      
      const status = service.getOfflineStatus();
      expect(status.hasPendingSync).toBe(true);
      expect(status.pendingItemsCount).toBe(1);
    });

    it('should update status when sync completes', async () => {
      service.addToOfflineQueue({
        type: 'assessment',
        action: 'create',
        data: {}
      });
      
      // Mock successful sync
      spyOn<any>(service, 'processQueueItem').and.resolveTo();
      
      await service.attemptSync();
      
      const status = service.getOfflineStatus();
      expect(status.hasPendingSync).toBe(false);
      expect(status.pendingItemsCount).toBe(0);
      expect(status.syncInProgress).toBe(false);
    });

    it('should update status when sync fails', async () => {
      service.addToOfflineQueue({
        type: 'assessment',
        action: 'create',
        data: {}
      });
      
      // Mock failed sync
      spyOn<any>(service, 'processQueueItem').and.rejectWith(new Error('Network error'));
      
      await service.attemptSync();
      
      const status = service.getOfflineStatus();
      expect(status.hasPendingSync).toBe(true);
      expect(status.pendingItemsCount).toBe(1);
      expect(status.syncInProgress).toBe(false);
    });
  });

  describe('Integration with Browser Events', () => {
    it('should handle online event', (done) => {
      service.onlineStatus$.subscribe(isOnline => {
        if (isOnline) {
          expect(service.isOnline()).toBe(true);
          done();
        }
      });
      
      // Simulate online event
      window.dispatchEvent(new Event('online'));
    });

    it('should handle offline event', (done) => {
      service.onlineStatus$.subscribe(isOnline => {
        if (!isOnline) {
          expect(service.isOnline()).toBe(false);
          done();
        }
      });
      
      // Simulate offline event
      window.dispatchEvent(new Event('offline'));
    });

    it('should attempt sync when coming back online', (done) => {
      spyOn(service, 'attemptSync').and.resolveTo();
      
      service.onlineStatus$.subscribe(isOnline => {
        if (isOnline) {
          expect(service.attemptSync).toHaveBeenCalled();
          done();
        }
      });
      
      // Simulate online event
      window.dispatchEvent(new Event('online'));
    });
  });

  describe('Error Handling', () => {
    it('should handle localStorage errors gracefully', () => {
      spyOn(console, 'error');
      spyOn(localStorage, 'setItem').and.throwError('Storage error');
      
      service.addToOfflineQueue({
        type: 'assessment',
        action: 'create',
        data: {}
      });
      
      expect(console.error).toHaveBeenCalledWith('Error saving offline queue:', jasmine.any(Error));
    });

    it('should handle sync errors gracefully', async () => {
      spyOn(console, 'error');
      
      service.addToOfflineQueue({
        type: 'assessment',
        action: 'create',
        data: {}
      });
      
      // Mock processQueueItems to throw error
      spyOn<any>(service, 'processQueueItems').and.rejectWith(new Error('Sync error'));
      
      await service.attemptSync();
      
      expect(console.error).toHaveBeenCalledWith('Error during offline sync:', jasmine.any(Error));
    });
  });
}); 