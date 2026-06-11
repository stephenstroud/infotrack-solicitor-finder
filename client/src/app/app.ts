import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { LocationsEditor } from './components/locations-editor';
import { ReportView } from './components/report-view';
import { SearchReport } from './models/report';
import { SolicitorApi } from './services/solicitor-api';

@Component({
  selector: 'app-root',
  imports: [LocationsEditor, ReportView, DatePipe],
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly api = inject(SolicitorApi);

  readonly locations = signal<string[]>([]);
  readonly report = signal<SearchReport | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.api.getDefaultLocations().subscribe({
      next: (locations) => this.locations.set(locations),
      error: () => this.error.set('Could not reach the API. Is it running on http://localhost:5211?'),
    });
  }

  search(): void {
    if (this.locations().length === 0) {
      this.error.set('Add at least one location first.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.api.search(this.locations()).subscribe({
      next: (report) => {
        this.report.set(report);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Search failed. Check the API is running on http://localhost:5211.');
        this.loading.set(false);
      },
    });
  }
}
