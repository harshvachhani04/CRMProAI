import { Injectable, inject, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';

export interface BreadcrumbItem {
  label: string;
  url: string;
}

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);

  private readonly _breadcrumbs = signal<BreadcrumbItem[]>([]);
  public readonly breadcrumbs = this._breadcrumbs.asReadonly();

  constructor() {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        const root = this.activatedRoute.root;
        const crumbs: BreadcrumbItem[] = [];
        this.buildBreadcrumbs(root, '', crumbs);
        this._breadcrumbs.set(crumbs);
      });
  }

  private buildBreadcrumbs(route: ActivatedRoute, url: string, breadcrumbs: BreadcrumbItem[]): void {
    const children: ActivatedRoute[] = route.children;

    if (children.length === 0) {
      return;
    }

    for (const child of children) {
      const routeURL: string = child.snapshot.url.map(segment => segment.path).join('/');
      let nextUrl = url;
      if (routeURL !== '') {
        nextUrl = `${url}/${routeURL}`;
      }

      const label = child.snapshot.data['breadcrumb'];
      if (label && !breadcrumbs.some(b => b.url === nextUrl && b.label === label)) {
        breadcrumbs.push({ label, url: nextUrl });
      }

      this.buildBreadcrumbs(child, nextUrl, breadcrumbs);
    }
  }

  setCustomBreadcrumbs(crumbs: BreadcrumbItem[]): void {
    this._breadcrumbs.set(crumbs);
  }
}
