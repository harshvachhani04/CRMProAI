import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AppShellComponent } from './app-shell.component';
import { routes } from '../../app.routes';

describe('AppShellComponent', () => {
  let component: AppShellComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppShellComponent],
      providers: [provideRouter(routes)]
    }).compileComponents();

    const fixture = TestBed.createComponent(AppShellComponent);
    component = fixture.componentInstance;
  });

  it('should create the app shell component', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle sidebar correctly', () => {
    expect(component.sidebarOpen()).toBe(false);
    component.toggleSidebar();
    expect(component.sidebarOpen()).toBe(true);
    component.toggleSidebar();
    expect(component.sidebarOpen()).toBe(false);
  });
});
