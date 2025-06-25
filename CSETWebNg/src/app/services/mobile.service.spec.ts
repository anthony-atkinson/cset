import { TestBed } from '@angular/core/testing';
import { MobileService } from './mobile.service';

describe('MobileService', () => {
  let service: MobileService;
  let mockWindow: any;

  beforeEach(() => {
    // Mock window object
    mockWindow = {
      innerWidth: 1024,
      innerHeight: 768,
      matchMedia: jasmine.createSpy('matchMedia').and.returnValue({
        matches: false,
        addListener: jasmine.createSpy('addListener'),
        removeListener: jasmine.createSpy('removeListener')
      }),
      addEventListener: jasmine.createSpy('addEventListener'),
      removeEventListener: jasmine.createSpy('removeEventListener')
    };

    // Mock navigator
    const mockNavigator = {
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
    };

    TestBed.configureTestingModule({
      providers: [MobileService]
    });

    service = TestBed.inject(MobileService);
    
    // Replace window with mock
    spyOnProperty(window, 'innerWidth').and.returnValue(mockWindow.innerWidth);
    spyOnProperty(window, 'innerHeight').and.returnValue(mockWindow.innerHeight);
    spyOn(window, 'matchMedia').and.callFake(mockWindow.matchMedia);
    spyOn(window, 'addEventListener').and.callFake(mockWindow.addEventListener);
    spyOn(window, 'removeEventListener').and.callFake(mockWindow.removeEventListener);
    spyOnProperty(navigator, 'userAgent').and.returnValue(mockNavigator.userAgent);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Device Detection', () => {
    it('should detect mobile devices by screen size', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(375);
      spyOnProperty(window, 'innerHeight').and.returnValue(667);
      
      const newService = new MobileService();
      expect(newService.isMobile()).toBe(true);
    });

    it('should detect tablet devices by screen size', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(768);
      spyOnProperty(window, 'innerHeight').and.returnValue(1024);
      
      const newService = new MobileService();
      expect(newService.isTablet()).toBe(true);
    });

    it('should detect desktop devices by screen size', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(1920);
      spyOnProperty(window, 'innerHeight').and.returnValue(1080);
      
      const newService = new MobileService();
      expect(newService.isDesktop()).toBe(true);
    });

    it('should detect touch capability', () => {
      spyOnProperty(navigator, 'maxTouchPoints').and.returnValue(5);
      
      const newService = new MobileService();
      expect(newService.isTouchCapable()).toBe(true);
    });

    it('should detect no touch capability', () => {
      spyOnProperty(navigator, 'maxTouchPoints').and.returnValue(0);
      
      const newService = new MobileService();
      expect(newService.isTouchCapable()).toBe(false);
    });
  });

  describe('Screen Size Detection', () => {
    it('should get screen width', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(375);
      
      const newService = new MobileService();
      expect(newService.getScreenWidth()).toBe(375);
    });

    it('should get screen height', () => {
      spyOnProperty(window, 'innerHeight').and.returnValue(667);
      
      const newService = new MobileService();
      expect(newService.getScreenHeight()).toBe(667);
    });

    it('should get viewport dimensions', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(1200);
      spyOnProperty(window, 'innerHeight').and.returnValue(800);
      
      const newService = new MobileService();
      const dimensions = newService.getViewportDimensions();
      expect(dimensions.width).toBe(1200);
      expect(dimensions.height).toBe(800);
    });
  });

  describe('Orientation Detection', () => {
    it('should detect portrait orientation', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(375);
      spyOnProperty(window, 'innerHeight').and.returnValue(667);
      
      const newService = new MobileService();
      expect(newService.isPortrait()).toBe(true);
      expect(newService.isLandscape()).toBe(false);
    });

    it('should detect landscape orientation', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(667);
      spyOnProperty(window, 'innerHeight').and.returnValue(375);
      
      const newService = new MobileService();
      expect(newService.isPortrait()).toBe(false);
      expect(newService.isLandscape()).toBe(true);
    });
  });

  describe('Breakpoint Detection', () => {
    it('should detect mobile breakpoint', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(480);
      
      const newService = new MobileService();
      expect(newService.isBelowBreakpoint('mobileSmall')).toBe(false);
      expect(newService.isBelowBreakpoint('mobileMedium')).toBe(true);
      expect(newService.isAboveBreakpoint('mobileSmall')).toBe(false);
    });

    it('should detect tablet breakpoint', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(768);
      
      const newService = new MobileService();
      expect(newService.isBelowBreakpoint('tablet')).toBe(true);
      expect(newService.isAboveBreakpoint('mobileMedium')).toBe(false);
    });

    it('should detect desktop breakpoint', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(1024);
      
      const newService = new MobileService();
      expect(newService.isBelowBreakpoint('tablet')).toBe(false);
      expect(newService.isAboveBreakpoint('tablet')).toBe(false);
    });
  });

  describe('Device Information', () => {
    it('should get device type', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(375);
      
      const newService = new MobileService();
      expect(newService.getDeviceType()).toBe('mobile');
    });

    it('should get recommended layout mode', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(768);
      
      const newService = new MobileService();
      expect(newService.getRecommendedLayoutMode()).toBe('tablet');
    });

    it('should check iOS device', () => {
      spyOnProperty(navigator, 'userAgent').and.returnValue(
        'Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15'
      );
      
      const newService = new MobileService();
      expect(newService.isIOS()).toBe(true);
    });

    it('should check Android device', () => {
      spyOnProperty(navigator, 'userAgent').and.returnValue(
        'Mozilla/5.0 (Linux; Android 10; SM-G975F) AppleWebKit/537.36'
      );
      
      const newService = new MobileService();
      expect(newService.isAndroid()).toBe(true);
    });

    it('should check Safari browser', () => {
      spyOnProperty(navigator, 'userAgent').and.returnValue(
        'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 Version/14.0 Safari/605.1.15'
      );
      
      const newService = new MobileService();
      expect(newService.isSafari()).toBe(true);
    });
  });

  describe('Observables and Events', () => {
    it('should provide device info observable', (done) => {
      service.deviceInfo$.subscribe(deviceInfo => {
        expect(deviceInfo).toBeDefined();
        expect(deviceInfo.isMobile).toBeDefined();
        expect(deviceInfo.isTablet).toBeDefined();
        expect(deviceInfo.isDesktop).toBeDefined();
        expect(deviceInfo.screenWidth).toBeDefined();
        expect(deviceInfo.screenHeight).toBeDefined();
        expect(deviceInfo.orientation).toBeDefined();
        expect(deviceInfo.touchCapable).toBeDefined();
        done();
      });
    });

    it('should handle resize events', (done) => {
      let callCount = 0;
      service.deviceInfo$.subscribe(() => {
        callCount++;
        if (callCount >= 2) {
          done();
        }
      });
      
      // Trigger resize event
      service.onResize();
    });

    it('should handle orientation change events', (done) => {
      let callCount = 0;
      service.deviceInfo$.subscribe(() => {
        callCount++;
        if (callCount >= 2) {
          done();
        }
      });
      
      // Trigger orientation change event
      service.onOrientationChange();
    });
  });

  describe('Utility Methods', () => {
    it('should get optimal touch target size', () => {
      spyOnProperty(navigator, 'maxTouchPoints').and.returnValue(5);
      
      const newService = new MobileService();
      const touchSize = newService.getOptimalTouchTargetSize();
      expect(touchSize).toBe(44); // Apple's recommended minimum
    });

    it('should get optimal font size for mobile', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(375);
      
      const newService = new MobileService();
      const fontSize = newService.getOptimalFontSize();
      expect(fontSize).toBe(16); // Prevent zoom on iOS
    });

    it('should get optimal font size for tablet', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(768);
      
      const newService = new MobileService();
      const fontSize = newService.getOptimalFontSize();
      expect(fontSize).toBe(14);
    });

    it('should get optimal font size for desktop', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(1920);
      
      const newService = new MobileService();
      const fontSize = newService.getOptimalFontSize();
      expect(fontSize).toBe(16);
    });

    it('should check if element is in viewport', () => {
      const mockElement = {
        getBoundingClientRect: () => ({
          top: 100,
          left: 100,
          bottom: 200,
          right: 200,
          width: 100,
          height: 100
        })
      } as HTMLElement;
      
      spyOnProperty(window, 'innerHeight').and.returnValue(800);
      spyOnProperty(window, 'innerWidth').and.returnValue(1200);
      
      const isInViewport = service.isElementInViewport(mockElement);
      expect(isInViewport).toBe(true);
    });

    it('should check if element is not in viewport', () => {
      const mockElement = {
        getBoundingClientRect: () => ({
          top: 1000,
          left: 100,
          bottom: 1100,
          right: 200,
          width: 100,
          height: 100
        })
      } as HTMLElement;
      
      spyOnProperty(window, 'innerHeight').and.returnValue(800);
      spyOnProperty(window, 'innerWidth').and.returnValue(1200);
      
      const isInViewport = service.isElementInViewport(mockElement);
      expect(isInViewport).toBe(false);
    });

    it('should scroll element into view', () => {
      const mockElement = {
        scrollIntoView: jasmine.createSpy('scrollIntoView')
      } as any;
      
      service.scrollIntoView(mockElement);
      expect(mockElement.scrollIntoView).toHaveBeenCalled();
    });

    it('should scroll element into view with options', () => {
      const mockElement = {
        scrollIntoView: jasmine.createSpy('scrollIntoView')
      } as any;
      
      const options: ScrollIntoViewOptions = { behavior: 'smooth', block: 'center' as ScrollLogicalPosition };
      service.scrollIntoView(mockElement, options);
      expect(mockElement.scrollIntoView).toHaveBeenCalledWith(options);
    });
  });

  describe('Feature Support', () => {
    it('should check feature support', () => {
      const supportsTouch = service.supportsFeature('touch');
      expect(typeof supportsTouch).toBe('boolean');
    });

    it('should check viewport support', () => {
      const supportsViewport = service.supportsFeature('viewport');
      expect(typeof supportsViewport).toBe('boolean');
    });

    it('should check orientation support', () => {
      const supportsOrientation = service.supportsFeature('orientation');
      expect(typeof supportsOrientation).toBe('boolean');
    });
  });

  describe('Viewport Management', () => {
    it('should prevent zoom on focus', () => {
      spyOn(document, 'querySelector').and.returnValue({
        setAttribute: jasmine.createSpy('setAttribute')
      } as any);
      
      service.preventZoomOnFocus();
      expect(document.querySelector).toHaveBeenCalledWith('meta[name="viewport"]');
    });

    it('should restore viewport', () => {
      spyOn(document, 'querySelector').and.returnValue({
        setAttribute: jasmine.createSpy('setAttribute')
      } as any);
      
      service.restoreViewport();
      expect(document.querySelector).toHaveBeenCalledWith('meta[name="viewport"]');
    });

    it('should add mobile classes', () => {
      spyOn(document.body.classList, 'add');
      
      service.addMobileClasses();
      expect(document.body.classList.add).toHaveBeenCalled();
    });
  });

  describe('Initialization', () => {
    it('should initialize service', () => {
      spyOn(service, 'addMobileClasses');
      
      service.initialize();
      expect(service.addMobileClasses).toHaveBeenCalled();
    });
  });

  describe('Error Handling', () => {
    it('should handle missing navigator gracefully', () => {
      spyOnProperty(navigator, 'userAgent').and.returnValue(undefined);
      
      const newService = new MobileService();
      expect(newService.isIOS()).toBe(false);
      expect(newService.isAndroid()).toBe(false);
      expect(newService.isSafari()).toBe(false);
    });

    it('should handle missing window properties gracefully', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(undefined);
      spyOnProperty(window, 'innerHeight').and.returnValue(undefined);
      
      const newService = new MobileService();
      expect(newService.getScreenWidth()).toBe(0);
      expect(newService.getScreenHeight()).toBe(0);
    });
  });
}); 