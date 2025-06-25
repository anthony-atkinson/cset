import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';
import { CollaborationService, CollaborationPermission, ConflictResolutionStrategy, CollaborativeComment, CollaborationStatistics, CollaborationActivity, UserPresence, AssessmentUpdate, EditingIndicator, ConflictResolution } from './collaboration.service';
import { ConfigService } from './config.service';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';
import { of, throwError } from 'rxjs';

describe('CollaborationService', () => {
  let service: CollaborationService;
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;
  let configService: jasmine.SpyObj<ConfigService>;
  let mockHubConnection: jasmine.SpyObj<HubConnection>;

  const mockApiUrl = 'http://localhost:5000/api/';
  const mockHubUrl = 'http://localhost:5000/collaborationHub';

  beforeEach(() => {
    const configSpy = jasmine.createSpyObj('ConfigService', [], { 
      apiUrl: mockApiUrl,
      hubUrl: mockHubUrl
    });

    // Mock SignalR HubConnection
    mockHubConnection = jasmine.createSpyObj('HubConnection', [
      'start',
      'stop',
      'invoke',
      'on',
      'off',
      'send',
      'stream',
      'onreconnecting',
      'onreconnected',
      'onclose'
    ], {
      state: HubConnectionState.Connected,
      connectionId: 'test-connection-id'
    });

    // Mock SignalR module
    const mockSignalR = {
      HubConnectionBuilder: jasmine.createSpy('HubConnectionBuilder').and.returnValue({
        withUrl: jasmine.createSpy('withUrl').and.returnValue({
          withAutomaticReconnect: jasmine.createSpy('withAutomaticReconnect').and.returnValue({
            configureLogging: jasmine.createSpy('configureLogging').and.returnValue({
              build: jasmine.createSpy('build').and.returnValue(mockHubConnection)
            })
          })
        })
      }),
      LogLevel: {
        Information: 2
      }
    };

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        CollaborationService,
        { provide: ConfigService, useValue: configSpy }
      ]
    });

    service = TestBed.inject(CollaborationService);
    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
    configService = TestBed.inject(ConfigService) as jasmine.SpyObj<ConfigService>;

    // Mock SignalR globally
    (window as any).signalR = mockSignalR;
  });

  afterEach(() => {
    httpMock.verify();
    service.ngOnDestroy();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('SignalR Connection Management', () => {
    it('should initialize connection on startup', () => {
      // The service initializes connection in constructor
      expect(mockHubConnection.on).toHaveBeenCalled();
      expect(mockHubConnection.start).toHaveBeenCalled();
    });

    it('should handle connection errors gracefully', async () => {
      mockHubConnection.start.and.rejectWith(new Error('Connection failed'));
      spyOn(console, 'error');

      // Create a new service instance to trigger connection
      const newService = new CollaborationService(configService, httpClient);

      expect(console.error).toHaveBeenCalledWith('Collaboration: Failed to start connection:', jasmine.any(Error));
    });

    it('should disconnect on destroy', () => {
      mockHubConnection.stop.and.resolveTo();

      service.ngOnDestroy();

      expect(mockHubConnection.stop).toHaveBeenCalled();
    });

    it('should provide connection state observable', (done) => {
      service.connectionState$.subscribe(connected => {
        expect(typeof connected).toBe('boolean');
        done();
      });
    });
  });

  describe('Assessment Collaboration', () => {
    beforeEach(() => {
      // Mock connection as established
      spyOn(service, 'isConnected').and.returnValue(true);
    });

    it('should join assessment', async () => {
      const assessmentId = 123;
      const userDisplayName = 'John Doe';

      await service.joinAssessment(assessmentId, userDisplayName);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('JoinAssessment', assessmentId, userDisplayName);
    });

    it('should handle join assessment errors', async () => {
      mockHubConnection.invoke.and.rejectWith(new Error('Join failed'));
      spyOn(console, 'error');

      await service.joinAssessment(123, 'John Doe');

      expect(console.error).toHaveBeenCalledWith('Failed to join assessment:', jasmine.any(Error));
    });

    it('should leave assessment', async () => {
      spyOn(service, 'getCurrentAssessmentId').and.returnValue(123);

      await service.leaveAssessment();

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('LeaveAssessment', 123);
    });
  });

  describe('Real-time Messaging', () => {
    beforeEach(() => {
      spyOn(service, 'isConnected').and.returnValue(true);
    });

    it('should send assessment update', async () => {
      const updateData = {
        questionId: 123,
        answer: 'Yes',
        notes: 'Updated answer'
      };

      await service.sendAssessmentUpdate('question', updateData);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('SendAssessmentUpdate', 'question', updateData);
    });

    it('should send collaborative comment', async () => {
      const comment = {
        assessmentId: 123,
        userId: 'user123',
        comment: 'This needs further investigation',
        timestamp: new Date()
      };

      await service.sendComment(comment);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('SendComment', comment);
    });

    it('should send editing indicator', async () => {
      const indicator = {
        questionId: 123,
        field: 'answer',
        isEditing: true
      };

      await service.sendEditingIndicator(indicator);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('SendEditingIndicator', indicator);
    });

    it('should handle messaging errors', async () => {
      mockHubConnection.invoke.and.rejectWith(new Error('Send failed'));
      spyOn(console, 'error');

      await service.sendAssessmentUpdate('question', {});

      expect(console.error).toHaveBeenCalledWith('Failed to send assessment update:', jasmine.any(Error));
    });
  });

  describe('Event Observables', () => {
    beforeEach(() => {
      mockHubConnection.on.and.callFake((method: string, callback: Function) => {
        // Store callback for later invocation
        (mockHubConnection.on as any)[method] = callback;
      });
    });

    it('should emit user joined events', (done) => {
      const mockUser: UserPresence = {
        userId: 'user123',
        displayName: 'John Doe',
        assessmentId: 123,
        connectedAt: new Date(),
        lastActivity: new Date(),
        isOnline: true
      };

      service.userJoined$.subscribe(user => {
        expect(user).toEqual(mockUser);
        done();
      });

      // Simulate user joined event
      (mockHubConnection.on as any)['UserJoined'](mockUser);
    });

    it('should emit user left events', (done) => {
      const mockUser: UserPresence = {
        userId: 'user123',
        displayName: 'John Doe',
        assessmentId: 123,
        connectedAt: new Date(),
        lastActivity: new Date(),
        isOnline: false
      };

      service.userLeft$.subscribe(user => {
        expect(user).toEqual(mockUser);
        done();
      });

      // Simulate user left event
      (mockHubConnection.on as any)['UserLeft'](mockUser);
    });

    it('should emit assessment updates', (done) => {
      const mockUpdate: AssessmentUpdate = {
        assessmentId: 123,
        updateType: 'question',
        updateData: { questionId: 123, answer: 'Yes' },
        userId: 'user123',
        timestamp: new Date()
      };

      service.assessmentUpdates$.subscribe(update => {
        expect(update).toEqual(mockUpdate);
        done();
      });

      // Simulate assessment update event
      (mockHubConnection.on as any)['AssessmentUpdated'](mockUpdate);
    });

    it('should emit comments', (done) => {
      const mockComment: CollaborativeComment = {
        assessmentId: 123,
        userId: 'user123',
        comment: 'Test comment',
        timestamp: new Date()
      };

      service.comments$.subscribe(comment => {
        expect(comment).toEqual(mockComment);
        done();
      });

      // Simulate comment event
      (mockHubConnection.on as any)['CommentAdded'](mockComment);
    });

    it('should emit editing indicators', (done) => {
      const mockIndicator: EditingIndicator = {
        assessmentId: 123,
        userId: 'user123',
        editingInfo: {
          questionId: 123,
          field: 'answer',
          isEditing: true
        },
        timestamp: new Date()
      };

      service.editingIndicators$.subscribe(indicator => {
        expect(indicator).toEqual(mockIndicator);
        done();
      });

      // Simulate editing indicator event
      (mockHubConnection.on as any)['UserEditing'](mockIndicator);
    });

    it('should emit conflict resolutions', (done) => {
      const mockResolution: ConflictResolution = {
        assessmentId: 123,
        resolvedBy: 'user123',
        conflictData: { questionId: 123, answer: 'Yes' },
        timestamp: new Date()
      };

      service.conflictResolutions$.subscribe(resolution => {
        expect(resolution).toEqual(mockResolution);
        done();
      });

      // Simulate conflict resolution event
      (mockHubConnection.on as any)['ConflictResolved'](mockResolution);
    });

    it('should emit active users updates', (done) => {
      const mockUsers: UserPresence[] = [
        {
          userId: 'user123',
          displayName: 'John Doe',
          assessmentId: 123,
          connectedAt: new Date(),
          lastActivity: new Date(),
          isOnline: true
        },
        {
          userId: 'user456',
          displayName: 'Jane Smith',
          assessmentId: 123,
          connectedAt: new Date(),
          lastActivity: new Date(),
          isOnline: true
        }
      ];

      service.activeUsers$.subscribe(users => {
        expect(users).toEqual(mockUsers);
        done();
      });

      // Simulate active users update event
      (mockHubConnection.on as any)['AssessmentJoined'](123, mockUsers);
    });
  });

  describe('Conflict Resolution', () => {
    it('should resolve conflict via API', () => {
      const assessmentId = 123;
      const conflictData = { questionId: 123, conflictingAnswers: ['Yes', 'No'] };
      const resolutionStrategy = ConflictResolutionStrategy.KeepLatest;

      const mockResult = {
        success: true,
        resolvedData: { questionId: 123, answer: 'Yes' }
      };

      service.resolveConflictViaApi(assessmentId, conflictData, resolutionStrategy).subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}collaboration/resolve-conflict/${assessmentId}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        conflictData,
        resolutionStrategy
      });
      req.flush(mockResult);
    });

    it('should handle conflict resolution errors', () => {
      const assessmentId = 123;
      const conflictData = {};
      const resolutionStrategy = ConflictResolutionStrategy.KeepLatest;

      service.resolveConflictViaApi(assessmentId, conflictData, resolutionStrategy).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.status).toBe(400);
          expect(error.error.message).toContain('Invalid conflict data');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}collaboration/resolve-conflict/${assessmentId}`);
      req.flush(
        { message: 'Invalid conflict data' },
        { status: 400, statusText: 'Bad Request' }
      );
    });
  });

  describe('API Integration', () => {
    it('should get collaboration history', () => {
      const assessmentId = 123;
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-01-31');

      const mockHistory: CollaborationActivity[] = [
        {
          id: 1,
          assessmentId: 123,
          userId: 'user123',
          activityType: 'UserJoined',
          activityData: '{"displayName": "John Doe"}',
          timestamp: new Date()
        }
      ];

      service.getCollaborationHistory(assessmentId, startDate, endDate).subscribe(history => {
        expect(history).toEqual(mockHistory);
      });

      const req = httpMock.expectOne(
        `${mockApiUrl}collaboration/history/${assessmentId}?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockHistory);
    });

    it('should get collaboration statistics', () => {
      const assessmentId = 123;

      const mockStats: CollaborationStatistics = {
        assessmentId: 123,
        totalActivities: 50,
        uniqueUsers: 5,
        lastActivity: new Date(),
        mostActiveUser: 'user123',
        conflictCount: 2
      };

      service.getCollaborationStatistics(assessmentId).subscribe(stats => {
        expect(stats).toEqual(mockStats);
      });

      const req = httpMock.expectOne(`${mockApiUrl}collaboration/statistics/${assessmentId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockStats);
    });

    it('should get user permissions', () => {
      const assessmentId = 123;

      const mockPermissions = {
        canView: true,
        canEdit: true,
        canComment: true,
        canResolveConflicts: false,
        canViewHistory: true
      };

      service.getUserPermissions(assessmentId).subscribe(permissions => {
        expect(permissions).toEqual(mockPermissions);
      });

      const req = httpMock.expectOne(`${mockApiUrl}collaboration/permissions/${assessmentId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockPermissions);
    });

    it('should record activity', () => {
      const assessmentId = 123;
      const activityType = 'UserJoined';
      const activityData = { displayName: 'John Doe' };

      const mockResult = { success: true, activityId: 456 };

      service.recordActivity(assessmentId, activityType, activityData).subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}collaboration/record-activity/${assessmentId}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ activityType, activityData });
      req.flush(mockResult);
    });
  });

  describe('Error Handling', () => {
    it('should handle HTTP errors gracefully', () => {
      service.getCollaborationStatistics(123).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.message).toContain('Internal Server Error');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}collaboration/statistics/123`);
      req.flush('Internal Server Error', { status: 500, statusText: 'Internal Server Error' });
    });

    it('should handle network errors', () => {
      service.getCollaborationStatistics(123).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.name).toBe('NetworkError');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}collaboration/statistics/123`);
      req.error(new ErrorEvent('NetworkError'));
    });
  });

  describe('Utility Methods', () => {
    it('should check connection status', () => {
      spyOn(service, 'isConnected').and.returnValue(true);
      expect(service.isConnected()).toBe(true);
    });

    it('should get current assessment ID', () => {
      spyOn(service, 'getCurrentAssessmentId').and.returnValue(123);
      expect(service.getCurrentAssessmentId()).toBe(123);
    });
  });
}); 