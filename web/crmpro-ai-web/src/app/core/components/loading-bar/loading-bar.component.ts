import { Component, inject } from '@angular/core';
import { LoadingService } from '../../services/loading.service';

@Component({
  selector: 'app-loading-bar',
  standalone: true,
  template: `
    @if (loadingService.isLoading()) {
      <div class="loading-bar-track">
        <div class="loading-bar-indeterminate"></div>
      </div>
    }
  `,
  styles: [`
    .loading-bar-track {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      height: 3px;
      background: rgba(118, 103, 232, 0.15);
      z-index: 99999;
      overflow: hidden;
    }
    .loading-bar-indeterminate {
      width: 100%;
      height: 100%;
      background: linear-gradient(90deg, #7667e8, #9b90ff, #7667e8);
      position: absolute;
      transform-origin: left;
      animation: indeterminate 1.4s infinite cubic-bezier(0.65, 0.815, 0.735, 0.395);
    }
    @keyframes indeterminate {
      0% {
        left: -100%;
        width: 100%;
      }
      100% {
        left: 100%;
        width: 10%;
      }
    }
  `]
})
export class LoadingBarComponent {
  readonly loadingService = inject(LoadingService);
}
