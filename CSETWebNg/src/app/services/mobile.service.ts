import { Injectable, HostListener } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface MobileBreakpoints {
  mobileSmall: number;
  mobileMedium: number;
  tablet: number;
  desktopSmall: number;
}

export interface DeviceInfo {
  isMobile: boolean;
  isTablet: boolean;
  isDesktop: boolean;
  screenWidth: number;
  screenHeight: number;
  orientation: 'portrait' | 'landscape';
  touchCapable: boolean;
  userAgent: string;
}

@Injectable({
  providedIn: 'root'
})
export class MobileService {
  private breakpoints: MobileBreakpoints = {
    mobileSmall: 480,
    mobileMedium: 768,
    tablet: 1024,
    desktopSmall: 1200
  };

  private deviceInfoSubject = new BehaviorSubject<DeviceInfo>(this.getDeviceInfo());
  public deviceInfo$: Observable<DeviceInfo> = this.deviceInfoSubject.asObservable();

  constructor() {
    this.updateDeviceInfo();
  }

  @HostListener('window:resize')
  onResize() {
    this.updateDeviceInfo();
  }

  @HostListener('window:orientationchange')
  onOrientationChange() {
    setTimeout(() => {
      this.updateDeviceInfo();
    }, 100);
  }

  /**
   * Get current device information
   */
  private getDeviceInfo(): DeviceInfo {
    const screenWidth = window.innerWidth;
    const screenHeight = window.innerHeight;
    const userAgent = navigator.userAgent.toLowerCase();

    return {
      isMobile: screenWidth <= this.breakpoints.mobileMedium,
      isTablet: screenWidth > this.breakpoints.mobileMedium && screenWidth <= this.breakpoints.tablet,
      isDesktop: screenWidth > this.breakpoints.tablet,
      screenWidth,
      screenHeight,
      orientation: screenWidth > screenHeight ? 'landscape' : 'portrait',
      touchCapable: this.hasTouchCapability(),
      userAgent
    };
  }

  /**
   * Update device information and notify subscribers
   */
  private updateDeviceInfo(): void {
    this.deviceInfoSubject.next(this.getDeviceInfo());
  }

  /**
   * Check if device supports touch
   */
  private hasTouchCapability(): boolean {
    return 'ontouchstart' in window || 
           navigator.maxTouchPoints > 0 || 
           (navigator as any).msMaxTouchPoints > 0;
  }

  /**
   * Check if current screen size is mobile
   */
  isMobile(): boolean {
    return this.deviceInfoSubject.value.isMobile;
  }

  /**
   * Check if current screen size is tablet
   */
  isTablet(): boolean {
    return this.deviceInfoSubject.value.isTablet;
  }

  /**
   * Check if current screen size is desktop
   */
  isDesktop(): boolean {
    return this.deviceInfoSubject.value.isDesktop;
  }

  /**
   * Check if device is in portrait orientation
   */
  isPortrait(): boolean {
    return this.deviceInfoSubject.value.orientation === 'portrait';
  }

  /**
   * Check if device is in landscape orientation
   */
  isLandscape(): boolean {
    return this.deviceInfoSubject.value.orientation === 'landscape';
  }

  /**
   * Check if device supports touch
   */
  isTouchCapable(): boolean {
    return this.deviceInfoSubject.value.touchCapable;
  }

  /**
   * Get current screen width
   */
  getScreenWidth(): number {
    return this.deviceInfoSubject.value.screenWidth;
  }

  /**
   * Get current screen height
   */
  getScreenHeight(): number {
    return this.deviceInfoSubject.value.screenHeight;
  }

  /**
   * Check if screen width is below a specific breakpoint
   */
  isBelowBreakpoint(breakpoint: keyof MobileBreakpoints): boolean {
    return this.getScreenWidth() <= this.breakpoints[breakpoint];
  }

  /**
   * Check if screen width is above a specific breakpoint
   */
  isAboveBreakpoint(breakpoint: keyof MobileBreakpoints): boolean {
    return this.getScreenWidth() > this.breakpoints[breakpoint];
  }

  /**
   * Get optimal font size for current device
   */
  getOptimalFontSize(): number {
    if (this.isMobile()) {
      return 16; // Prevent zoom on iOS
    } else if (this.isTablet()) {
      return 14;
    } else {
      return 16;
    }
  }

  /**
   * Get optimal touch target size for current device
   */
  getOptimalTouchTargetSize(): number {
    if (this.isTouchCapable()) {
      return 44; // Apple's recommended minimum
    } else {
      return 32; // Desktop minimum
    }
  }

  /**
   * Check if device is iOS
   */
  isIOS(): boolean {
    return /iphone|ipad|ipod/.test(this.deviceInfoSubject.value.userAgent);
  }

  /**
   * Check if device is Android
   */
  isAndroid(): boolean {
    return /android/.test(this.deviceInfoSubject.value.userAgent);
  }

  /**
   * Check if device is Safari
   */
  isSafari(): boolean {
    return /safari/.test(this.deviceInfoSubject.value.userAgent) && 
           !/chrome/.test(this.deviceInfoSubject.value.userAgent);
  }

  /**
   * Get device type string
   */
  getDeviceType(): string {
    if (this.isMobile()) {
      return 'mobile';
    } else if (this.isTablet()) {
      return 'tablet';
    } else {
      return 'desktop';
    }
  }

  /**
   * Get recommended layout mode for current device
   */
  getRecommendedLayoutMode(): 'mobile' | 'tablet' | 'desktop' {
    if (this.isMobile()) {
      return 'mobile';
    } else if (this.isTablet()) {
      return 'tablet';
    } else {
      return 'desktop';
    }
  }

  /**
   * Check if device supports specific features
   */
  supportsFeature(feature: string): boolean {
    switch (feature) {
      case 'touch':
        return this.isTouchCapable();
      case 'geolocation':
        return 'geolocation' in navigator;
      case 'camera':
        return 'mediaDevices' in navigator && 'getUserMedia' in navigator.mediaDevices;
      case 'localStorage':
        return 'localStorage' in window;
      case 'sessionStorage':
        return 'sessionStorage' in window;
      case 'serviceWorker':
        return 'serviceWorker' in navigator;
      case 'webGL':
        return 'WebGLRenderingContext' in window;
      default:
        return false;
    }
  }

  /**
   * Get viewport dimensions
   */
  getViewportDimensions(): { width: number; height: number } {
    return {
      width: window.innerWidth,
      height: window.innerHeight
    };
  }

  /**
   * Check if element is in viewport
   */
  isElementInViewport(element: HTMLElement): boolean {
    const rect = element.getBoundingClientRect();
    return (
      rect.top >= 0 &&
      rect.left >= 0 &&
      rect.bottom <= window.innerHeight &&
      rect.right <= window.innerWidth
    );
  }

  /**
   * Scroll element into view with mobile-friendly behavior
   */
  scrollIntoView(element: HTMLElement, options: ScrollIntoViewOptions = {}): void {
    const defaultOptions: ScrollIntoViewOptions = {
      behavior: 'smooth',
      block: 'center',
      inline: 'nearest'
    };

    element.scrollIntoView({ ...defaultOptions, ...options });
  }

  /**
   * Prevent zoom on input focus for iOS
   */
  preventZoomOnFocus(): void {
    if (this.isIOS()) {
      const viewport = document.querySelector('meta[name="viewport"]');
      if (viewport) {
        viewport.setAttribute('content', 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no');
      }
    }
  }

  /**
   * Restore normal viewport behavior
   */
  restoreViewport(): void {
    const viewport = document.querySelector('meta[name="viewport"]');
    if (viewport) {
      viewport.setAttribute('content', 'width=device-width, initial-scale=1');
    }
  }

  /**
   * Add mobile-specific CSS classes to body
   */
  addMobileClasses(): void {
    const body = document.body;
    const deviceInfo = this.deviceInfoSubject.value;

    // Remove existing classes
    body.classList.remove('mobile-device', 'tablet-device', 'desktop-device', 'touch-device', 'portrait', 'landscape');

    // Add appropriate classes
    if (deviceInfo.isMobile) {
      body.classList.add('mobile-device');
    } else if (deviceInfo.isTablet) {
      body.classList.add('tablet-device');
    } else {
      body.classList.add('desktop-device');
    }

    if (deviceInfo.touchCapable) {
      body.classList.add('touch-device');
    }

    if (deviceInfo.orientation === 'portrait') {
      body.classList.add('portrait');
    } else {
      body.classList.add('landscape');
    }
  }

  /**
   * Initialize mobile service
   */
  initialize(): void {
    this.addMobileClasses();
    this.updateDeviceInfo();
  }
} 