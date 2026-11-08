import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ToastContainerComponent } from './toast-container.component';
import { NotificationService } from '../../services/notification.service';

describe('ToastContainerComponent', () => {
  let component: ToastContainerComponent;
  let notificationService: NotificationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastContainerComponent],
      providers: [NotificationService]
    }).compileComponents();

    const fixture = TestBed.createComponent(ToastContainerComponent);
    component = fixture.componentInstance;
    notificationService = TestBed.inject(NotificationService);
  });

  it('should create the toast container', () => {
    expect(component).toBeTruthy();
  });

  it('should dismiss toast on request', () => {
    const id = notificationService.showSuccess('Test', 'Detail');
    expect(notificationService.messages().length).toBe(1);

    component.dismiss(id);
    expect(notificationService.messages().length).toBe(0);
  });
});
