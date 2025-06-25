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
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';
import { FileExportService } from './file-export.service';

export interface ExportOptions {
  format: 'json' | 'xml' | 'csv';
  includeData?: boolean;
  includeStructure?: boolean;
  prettyPrint?: boolean;
  encrypt?: boolean;
  password?: string;
}

export interface BulkExportRequest {
  assessmentIds: number[];
  format: 'json' | 'xml' | 'csv';
  includeData?: boolean;
  includeStructure?: boolean;
  prettyPrint?: boolean;
  encrypt?: boolean;
  password?: string;
}

export interface MergeExportRequest {
  assessmentIds: number[];
  format: 'json' | 'xml' | 'csv';
  mergeAnswers?: boolean;
  mergeFindings?: boolean;
  mergeDocuments?: boolean;
  conflictResolution?: 'KeepLatest' | 'KeepEarliest' | 'KeepMostComplete' | 'Manual';
  prettyPrint?: boolean;
  encrypt?: boolean;
  password?: string;
}

export interface ImportOptions {
  format: 'json' | 'xml' | 'csv';
  isTemplate?: boolean;
  isMerge?: boolean;
  targetAssessmentId?: number;
  isEncrypted?: boolean;
  password?: string;
  csvDelimiter?: string;
  validateOnly?: boolean;
}

export interface ImportResult {
  success: boolean;
  importedAssessmentId?: number;
  errors?: string[];
  warnings?: string[];
  messages?: string[];
}

export interface ValidationResult {
  isValid: boolean;
  errors?: string[];
  warnings?: string[];
  messages?: string[];
}

export interface FormatInfo {
  format: string;
  name: string;
  description: string;
  supportsEncryption: boolean;
  supportsPrettyPrint: boolean;
  supportsTemplates: boolean;
  supportsMerging: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class EnhancedExportImportService {
  private apiUrl: string;

  constructor(
    private http: HttpClient,
    private configSvc: ConfigService,
    private fileExportSvc: FileExportService
  ) {
    this.apiUrl = this.configSvc.apiUrl + 'enhanced/';
  }

  /**
   * Exports an assessment in the specified format with enhanced options
   */
  exportAssessment(options: ExportOptions): Observable<Blob> {
    const params = new HttpParams()
      .set('format', options.format)
      .set('includeData', options.includeData !== false)
      .set('includeStructure', options.includeStructure !== false)
      .set('prettyPrint', options.prettyPrint !== false)
      .set('encrypt', options.encrypt || false);

    if (options.password) {
      params.set('password', options.password);
    }

    return this.http.get(this.apiUrl + 'export', {
      params: params,
      responseType: 'blob'
    });
  }

  /**
   * Exports multiple assessments in bulk
   */
  bulkExportAssessments(request: BulkExportRequest): Observable<Blob> {
    return this.http.post(this.apiUrl + 'export/bulk', request, {
      responseType: 'blob'
    });
  }

  /**
   * Exports an assessment template (structure without data)
   */
  exportTemplate(options: ExportOptions): Observable<Blob> {
    const params = new HttpParams()
      .set('format', options.format)
      .set('prettyPrint', options.prettyPrint !== false)
      .set('encrypt', options.encrypt || false);

    if (options.password) {
      params.set('password', options.password);
    }

    return this.http.get(this.apiUrl + 'export/template', {
      params: params,
      responseType: 'blob'
    });
  }

  /**
   * Merges multiple assessments into a single export
   */
  mergeAssessments(request: MergeExportRequest): Observable<Blob> {
    return this.http.post(this.apiUrl + 'export/merge', request, {
      responseType: 'blob'
    });
  }

  /**
   * Imports an assessment from the specified format with enhanced validation
   */
  importAssessment(file: File, options: ImportOptions): Observable<ImportResult> {
    const formData = new FormData();
    formData.append('file', file);

    const params = new HttpParams()
      .set('format', options.format)
      .set('isTemplate', options.isTemplate || false)
      .set('isMerge', options.isMerge || false)
      .set('isEncrypted', options.isEncrypted || false)
      .set('csvDelimiter', options.csvDelimiter || ',')
      .set('validateOnly', options.validateOnly || false);

    if (options.targetAssessmentId) {
      params.set('targetAssessmentId', options.targetAssessmentId.toString());
    }

    if (options.password) {
      params.set('password', options.password);
    }

    return this.http.post<ImportResult>(this.apiUrl + 'import', formData, { params });
  }

  /**
   * Validates an import file without actually importing it
   */
  validateImport(file: File, options: ImportOptions): Observable<ValidationResult> {
    const formData = new FormData();
    formData.append('file', file);

    const params = new HttpParams()
      .set('format', options.format)
      .set('isEncrypted', options.isEncrypted || false)
      .set('csvDelimiter', options.csvDelimiter || ',');

    if (options.password) {
      params.set('password', options.password);
    }

    return this.http.post<ValidationResult>(this.apiUrl + 'import/validate', formData, { params });
  }

  /**
   * Gets available export formats and their capabilities
   */
  getSupportedFormats(): Observable<FormatInfo[]> {
    return this.http.get<FormatInfo[]>(this.apiUrl + 'formats');
  }

  /**
   * Downloads a file from blob response
   */
  downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Exports assessment and automatically downloads the file
   */
  exportAndDownload(options: ExportOptions, filename?: string): void {
    this.exportAssessment(options).subscribe(
      (blob: Blob) => {
        const defaultFilename = `assessment_export_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.${options.format}`;
        this.downloadFile(blob, filename || defaultFilename);
      },
      (error) => {
        console.error('Export failed:', error);
        // Handle error (show notification, etc.)
      }
    );
  }

  /**
   * Bulk exports assessments and automatically downloads the ZIP file
   */
  bulkExportAndDownload(request: BulkExportRequest): void {
    this.bulkExportAssessments(request).subscribe(
      (blob: Blob) => {
        const filename = `bulk_export_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.zip`;
        this.downloadFile(blob, filename);
      },
      (error) => {
        console.error('Bulk export failed:', error);
        // Handle error (show notification, etc.)
      }
    );
  }

  /**
   * Exports template and automatically downloads the file
   */
  exportTemplateAndDownload(options: ExportOptions, filename?: string): void {
    this.exportTemplate(options).subscribe(
      (blob: Blob) => {
        const defaultFilename = `assessment_template_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.${options.format}`;
        this.downloadFile(blob, filename || defaultFilename);
      },
      (error) => {
        console.error('Template export failed:', error);
        // Handle error (show notification, etc.)
      }
    );
  }

  /**
   * Merges assessments and automatically downloads the file
   */
  mergeAndDownload(request: MergeExportRequest): void {
    this.mergeAssessments(request).subscribe(
      (blob: Blob) => {
        const filename = `merged_assessment_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.${request.format}`;
        this.downloadFile(blob, filename);
      },
      (error) => {
        console.error('Merge failed:', error);
        // Handle error (show notification, etc.)
      }
    );
  }

  /**
   * Creates a file input element and triggers file selection
   */
  selectFile(accept: string = '*.*'): Promise<File> {
    return new Promise((resolve, reject) => {
      const input = document.createElement('input');
      input.type = 'file';
      input.accept = accept;
      input.style.display = 'none';

      input.onchange = (event: any) => {
        const file = event.target.files[0];
        if (file) {
          resolve(file);
        } else {
          reject(new Error('No file selected'));
        }
        document.body.removeChild(input);
      };

      input.onerror = () => {
        reject(new Error('File selection failed'));
        document.body.removeChild(input);
      };

      document.body.appendChild(input);
      input.click();
    });
  }

  /**
   * Imports assessment from file with automatic format detection
   */
  importAssessmentFromFile(file: File, options: ImportOptions): Observable<ImportResult> {
    // Auto-detect format if not specified
    if (!options.format) {
      const extension = file.name.split('.').pop()?.toLowerCase();
      switch (extension) {
        case 'json':
          options.format = 'json';
          break;
        case 'xml':
          options.format = 'xml';
          break;
        case 'csv':
          options.format = 'csv';
          break;
        default:
          options.format = 'json'; // Default to JSON
      }
    }

    return this.importAssessment(file, options);
  }

  /**
   * Validates import file with automatic format detection
   */
  validateImportFile(file: File, options: ImportOptions): Observable<ValidationResult> {
    // Auto-detect format if not specified
    if (!options.format) {
      const extension = file.name.split('.').pop()?.toLowerCase();
      switch (extension) {
        case 'json':
          options.format = 'json';
          break;
        case 'xml':
          options.format = 'xml';
          break;
        case 'csv':
          options.format = 'csv';
          break;
        default:
          options.format = 'json'; // Default to JSON
      }
    }

    return this.validateImport(file, options);
  }
} 