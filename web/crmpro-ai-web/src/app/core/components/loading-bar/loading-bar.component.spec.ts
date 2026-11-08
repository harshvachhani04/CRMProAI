import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { LoadingBarComponent } from './loading-bar.component';
import { LoadingService } from '../../services/loading.service';

describe('LoadingBarComponent', () => {
  let component: LoadingBarComponent;
  let loadingService: LoadingService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoadingBarComponent],
      providers: [LoadingService]
    }).compileComponents();

    const fixture = TestBed.createComponent(LoadingBarComponent);
    component = fixture.componentInstance;
    loadingService = TestBed.inject(LoadingService);
  });

  it('should create the loading bar', () => {
    expect(component).toBeTruthy();
    expect(loadingService.isLoading()).toBe(false);
  });
});
