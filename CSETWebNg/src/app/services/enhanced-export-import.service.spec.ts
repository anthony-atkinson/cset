import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EnhancedExportImportService, ExportOptions, ImportResult, ImportOptions, BulkExportRequest, MergeExportRequest } from './enhanced-export-import.service';
import { ConfigService } from './config.service';
import { FileExportService } from './file-export.service';

describe('EnhancedExportImportService', () => {
  let service: EnhancedExportImportService;
  let httpMock: HttpTestingController;
  let configService: jasmine.SpyObj<ConfigService>;
  let fileExportService: jasmine.SpyObj<FileExportService>;

  const mockApiUrl = 'http://localhost:5000/api/';

  beforeEach(() => {
    const configSpy = jasmine.createSpyObj('ConfigService', [], { apiUrl: mockApiUrl });
    const fileExportSpy = jasmine.createSpyObj('FileExportService', ['downloadFile']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        EnhancedExportImportService,
        { provide: ConfigService, useValue: configSpy },
        { provide: FileExportService, useValue: fileExportSpy }
      ]
    });

    service = TestBed.inject(EnhancedExportImportService);
    httpMock = TestBed.inject(HttpTestingController);
    configService = TestBed.inject(ConfigService) as jasmine.SpyObj<ConfigService>;
    fileExportService = TestBed.inject(FileExportService) as jasmine.SpyObj<FileExportService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Assessment Export', () => {
    const mockBlob = new Blob(['test data'], { type: 'application/json' });

    it('should export assessment in JSON format', () => {
      const options: ExportOptions = { 
        format: 'json', 
        includeData: true, 
        includeStructure: true,
        prettyPrint: true 
      };

      service.exportAssessment(options).subscribe(result => {
        expect(result).toEqual(mockBlob);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export?format=json&includeData=true&includeStructure=true&prettyPrint=true&encrypt=false`);
      expect(req.request.method).toBe('GET');
      req.flush(mockBlob);
    });

    it('should export assessment in XML format', () => {
      const options: ExportOptions = { 
        format: 'xml', 
        includeData: false, 
        includeStructure: true,
        prettyPrint: false 
      };

      service.exportAssessment(options).subscribe(result => {
        expect(result).toEqual(mockBlob);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export?format=xml&includeData=false&includeStructure=true&prettyPrint=false&encrypt=false`);
      expect(req.request.method).toBe('GET');
      req.flush(mockBlob);
    });

    it('should export assessment in CSV format', () => {
      const options: ExportOptions = { 
        format: 'csv', 
        includeData: true, 
        includeStructure: false,
        prettyPrint: true 
      };

      service.exportAssessment(options).subscribe(result => {
        expect(result).toEqual(mockBlob);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export?format=csv&includeData=true&includeStructure=false&prettyPrint=true&encrypt=false`);
      expect(req.request.method).toBe('GET');
      req.flush(mockBlob);
    });

    it('should export assessment with encryption', () => {
      const options: ExportOptions = { 
        format: 'json', 
        includeData: true,
        encrypt: true, 
        password: 'testpassword' 
      };

      service.exportAssessment(options).subscribe(result => {
        expect(result).toEqual(mockBlob);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export?format=json&includeData=true&includeStructure=true&prettyPrint=true&encrypt=true&password=testpassword`);
      expect(req.request.method).toBe('GET');
      req.flush(mockBlob);
    });
  });

  describe('Bulk Export', () => {
    const mockBulkExportRequest: BulkExportRequest = {
      assessmentIds: [123, 456, 789],
      format: 'json',
      includeData: true,
      includeStructure: true
    };

    it('should export multiple assessments', () => {
      const mockBlob = new Blob(['bulk data'], { type: 'application/zip' });

      service.bulkExportAssessments(mockBulkExportRequest).subscribe(result => {
        expect(result).toEqual(mockBlob);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export/bulk`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockBulkExportRequest);
      req.flush(mockBlob);
    });

    it('should handle bulk export errors', () => {
      service.bulkExportAssessments(mockBulkExportRequest).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.status).toBe(400);
          expect(error.error.message).toContain('Invalid assessment IDs');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export/bulk`);
      req.flush(
        { message: 'Invalid assessment IDs' }, 
        { status: 400, statusText: 'Bad Request' }
      );
    });
  });

  describe('Assessment Import', () => {
    const mockImportData = {
      name: 'Imported Assessment',
      questions: [
        { questionId: 1, answer: 'Yes' },
        { questionId: 2, answer: 'No' }
      ]
    };

    it('should import assessment from JSON', () => {
      const file = new File([JSON.stringify(mockImportData)], 'test.json', { type: 'application/json' });
      const options: ImportOptions = { format: 'json' };

      const mockResult: ImportResult = {
        success: true,
        importedAssessmentId: 456,
        messages: ['Assessment imported successfully']
      };

      service.importAssessment(file, options).subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/import?format=json&isTemplate=false&isMerge=false&isEncrypted=false&csvDelimiter=%2C&validateOnly=false`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body instanceof FormData).toBe(true);
      req.flush(mockResult);
    });

    it('should import assessment from XML', () => {
      const xmlData = '<assessment><name>Test</name><questions><question><id>1</id><answer>Yes</answer></question></questions></assessment>';
      const file = new File([xmlData], 'test.xml', { type: 'application/xml' });
      const options: ImportOptions = { format: 'xml' };

      const mockResult: ImportResult = {
        success: true,
        messages: ['Assessment imported successfully']
      };

      service.importAssessment(file, options).subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/import?format=xml&isTemplate=false&isMerge=false&isEncrypted=false&csvDelimiter=%2C&validateOnly=false`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResult);
    });

    it('should import assessment from CSV', () => {
      const csvData = 'question,answer\n1,Yes\n2,No';
      const file = new File([csvData], 'test.csv', { type: 'text/csv' });
      const options: ImportOptions = { format: 'csv', csvDelimiter: ',' };

      const mockResult: ImportResult = {
        success: true,
        messages: ['Assessment imported successfully']
      };

      service.importAssessment(file, options).subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/import?format=csv&isTemplate=false&isMerge=false&isEncrypted=false&csvDelimiter=%2C&validateOnly=false`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResult);
    });

    it('should handle import validation errors', () => {
      const file = new File(['invalid data'], 'test.json', { type: 'application/json' });
      const options: ImportOptions = { format: 'json' };

      service.importAssessment(file, options).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.status).toBe(400);
          expect(error.error.errors).toBeDefined();
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/import?format=json&isTemplate=false&isMerge=false&isEncrypted=false&csvDelimiter=%2C&validateOnly=false`);
      req.flush(
        { 
          success: false, 
          errors: ['Invalid JSON format', 'Missing required fields'] 
        }, 
        { status: 400, statusText: 'Bad Request' }
      );
    });

    it('should import encrypted assessment', () => {
      const file = new File(['encrypted_data'], 'test.enc', { type: 'application/octet-stream' });
      const options: ImportOptions = { format: 'json', isEncrypted: true, password: 'testpassword' };

      const mockResult: ImportResult = {
        success: true,
        messages: ['Encrypted assessment imported successfully']
      };

      service.importAssessment(file, options).subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/import?format=json&isTemplate=false&isMerge=false&isEncrypted=true&csvDelimiter=%2C&validateOnly=false&password=testpassword`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResult);
    });
  });

  describe('Template Management', () => {
    it('should export assessment template', () => {
      const options: ExportOptions = { 
        format: 'json', 
        includeData: false, 
        includeStructure: true,
        prettyPrint: true 
      };

      const mockBlob = new Blob(['template data'], { type: 'application/json' });

      service.exportTemplate(options).subscribe(result => {
        expect(result).toEqual(mockBlob);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export/template?format=json&prettyPrint=true&encrypt=false`);
      expect(req.request.method).toBe('GET');
      req.flush(mockBlob);
    });

    it('should import assessment template', () => {
      const templateData = {
        templateId: 'template-123',
        name: 'Standard Assessment Template',
        questions: [
          { questionId: 1, category: 'Access Control' },
          { questionId: 2, category: 'Data Protection' }
        ]
      };

      const file = new File([JSON.stringify(templateData)], 'template.json', { type: 'application/json' });
      const options: ImportOptions = { format: 'json', isTemplate: true };

      const mockResult: ImportResult = {
        success: true,
        messages: ['Template imported successfully']
      };

      service.importAssessment(file, options).subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/import?format=json&isTemplate=true&isMerge=false&isEncrypted=false&csvDelimiter=%2C&validateOnly=false`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResult);
    });
  });

  describe('Assessment Merging', () => {
    const mockMergeRequest: MergeExportRequest = {
      assessmentIds: [123, 456],
      format: 'json',
      mergeAnswers: true,
      mergeFindings: true,
      mergeDocuments: false,
      conflictResolution: 'KeepLatest'
    };

    it('should merge assessments', () => {
      const mockBlob = new Blob(['merged data'], { type: 'application/json' });

      service.mergeAssessments(mockMergeRequest).subscribe(result => {
        expect(result).toEqual(mockBlob);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export/merge`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockMergeRequest);
      req.flush(mockBlob);
    });

    it('should handle merge conflicts', () => {
      service.mergeAssessments(mockMergeRequest).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.status).toBe(409);
          expect(error.error.conflicts).toBeDefined();
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export/merge`);
      req.flush(
        { 
          success: false, 
          conflicts: [
            { questionId: 1, sourceAnswer: 'Yes', targetAnswer: 'No' }
          ] 
        }, 
        { status: 409, statusText: 'Conflict' }
      );
    });
  });

  describe('File Operations', () => {
    it('should validate import file', () => {
      const file = new File(['{}'], 'test.json', { type: 'application/json' });
      const options: ImportOptions = { format: 'json', validateOnly: true };

      const mockValidationResult = {
        isValid: true,
        messages: ['File is valid']
      };

      service.validateImport(file, options).subscribe(result => {
        expect(result).toEqual(mockValidationResult);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/import?format=json&isTemplate=false&isMerge=false&isEncrypted=false&csvDelimiter=%2C&validateOnly=true`);
      expect(req.request.method).toBe('POST');
      req.flush(mockValidationResult);
    });

    it('should get supported formats', () => {
      const mockFormats = [
        {
          format: 'json',
          name: 'JSON',
          description: 'JavaScript Object Notation',
          supportsEncryption: true,
          supportsPrettyPrint: true,
          supportsTemplates: true,
          supportsMerging: true
        }
      ];

      service.getSupportedFormats().subscribe(formats => {
        expect(formats).toEqual(mockFormats);
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/formats`);
      expect(req.request.method).toBe('GET');
      req.flush(mockFormats);
    });

    it('should download file', () => {
      const mockBlob = new Blob(['test data'], { type: 'application/json' });
      const filename = 'test.json';

      service.downloadFile(mockBlob, filename);

      expect(fileExportService.downloadFile).toHaveBeenCalledWith(mockBlob, filename);
    });

    it('should export and download', () => {
      const options: ExportOptions = { format: 'json', includeData: true };
      const filename = 'assessment.json';

      spyOn(service, 'exportAssessment').and.returnValue(jasmine.createSpyObj('Observable', ['subscribe']));

      service.exportAndDownload(options, filename);

      expect(service.exportAssessment).toHaveBeenCalledWith(options);
    });
  });

  describe('Error Handling', () => {
    it('should handle network errors', () => {
      const options: ExportOptions = { format: 'json', includeData: true };

      service.exportAssessment(options).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.name).toBe('NetworkError');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export?format=json&includeData=true&includeStructure=true&prettyPrint=true&encrypt=false`);
      req.error(new ErrorEvent('NetworkError'));
    });

    it('should handle server errors', () => {
      const options: ExportOptions = { format: 'json', includeData: true };

      service.exportAssessment(options).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.status).toBe(500);
          expect(error.error.message).toContain('Internal Server Error');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export?format=json&includeData=true&includeStructure=true&prettyPrint=true&encrypt=false`);
      req.flush(
        { message: 'Internal Server Error' }, 
        { status: 500, statusText: 'Internal Server Error' }
      );
    });

    it('should handle timeout errors', () => {
      const options: ExportOptions = { format: 'json', includeData: true };

      service.exportAssessment(options).subscribe({
        next: () => fail('Should not succeed'),
        error: (error) => {
          expect(error.status).toBe(408);
          expect(error.error.message).toContain('Request Timeout');
        }
      });

      const req = httpMock.expectOne(`${mockApiUrl}enhanced/export?format=json&includeData=true&includeStructure=true&prettyPrint=true&encrypt=false`);
      req.flush(
        { message: 'Request Timeout' }, 
        { status: 408, statusText: 'Request Timeout' }
      );
    });
  });
}); 