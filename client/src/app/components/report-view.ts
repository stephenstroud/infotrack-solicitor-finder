import { Component, computed, input } from '@angular/core';
import { RankedFirm, SearchReport, SolicitorView } from '../models/report';

/** Renders a SearchReport: headline stats, contactability, breakdown, top-rated and the directory. */
@Component({
  selector: 'app-report-view',
  imports: [],
  template: `
    <!-- Headline cards -->
    <div class="grid grid-cols-2 gap-4 lg:grid-cols-4">
      <div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <p class="text-xs font-medium tracking-wide text-slate-500 uppercase">Firms found</p>
        <p class="mt-1 text-3xl font-semibold text-slate-900">{{ report().totalFirms }}</p>
      </div>
      <div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <p class="text-xs font-medium tracking-wide text-slate-500 uppercase">Locations</p>
        <p class="mt-1 text-3xl font-semibold text-slate-900">{{ report().locationsSearched }}</p>
      </div>
      <div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <p class="text-xs font-medium tracking-wide text-slate-500 uppercase">Reachable</p>
        <p class="mt-1 text-3xl font-semibold text-emerald-600">
          {{ report().contactability.reachablePercent }}%
        </p>
      </div>
      <div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <p class="text-xs font-medium tracking-wide text-slate-500 uppercase">Top location</p>
        <p class="mt-1 truncate text-2xl font-semibold text-slate-900">
          {{ report().topLocation ?? '—' }}
        </p>
      </div>
    </div>

    <!-- New-firm alert -->
    @if (report().hasNewFirms) {
      <div class="mt-4 rounded-xl border border-amber-300 bg-amber-50 p-4">
        <p class="font-semibold text-amber-900">
          {{ report().newFirms.length }} new firm(s) since the previous search
        </p>
        <div class="mt-2 flex flex-wrap gap-2">
          @for (firm of report().newFirms; track firm.name + firm.location) {
            <span class="rounded-full bg-amber-200 px-3 py-1 text-sm text-amber-900">
              {{ firm.name }} · {{ firm.location }}
            </span>
          }
        </div>
      </div>
    }

    <div class="mt-4 grid gap-4 lg:grid-cols-2">
      <!-- Contactability -->
      <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <h3 class="text-sm font-semibold text-slate-700">Contactability</h3>
        <p class="mb-3 text-xs text-slate-500">How reachable the gathered leads are.</p>
        @for (bar of contactBars(); track bar.label) {
          <div class="mb-2">
            <div class="flex justify-between text-xs text-slate-600">
              <span>{{ bar.label }}</span>
              <span>{{ bar.percent }}%</span>
            </div>
            <div class="mt-1 h-2 w-full overflow-hidden rounded-full bg-slate-100">
              <div class="h-full rounded-full bg-indigo-500" [style.width.%]="bar.percent"></div>
            </div>
          </div>
        }
      </div>

      <!-- Per-location breakdown -->
      <div class="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <h3 class="border-b border-slate-100 px-5 py-3 text-sm font-semibold text-slate-700">
          Firms by location
        </h3>
        <table class="w-full text-sm">
          <thead class="bg-slate-50 text-left text-xs text-slate-500 uppercase">
            <tr>
              <th class="px-5 py-2 font-medium">Location</th>
              <th class="px-3 py-2 text-right font-medium">Firms</th>
              <th class="px-3 py-2 text-right font-medium">Reachable</th>
              <th class="px-5 py-2 text-right font-medium">Phone</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            @for (row of report().breakdown; track row.location) {
              <tr>
                <td class="px-5 py-2 font-medium text-slate-800">{{ row.location }}</td>
                <td class="px-3 py-2 text-right text-slate-700">{{ row.firmCount }}</td>
                <td class="px-3 py-2 text-right text-slate-700">{{ row.reachablePercent }}%</td>
                <td class="px-5 py-2 text-right text-slate-700">{{ row.withPhone }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>

    <!-- Top rated -->
    @if (report().topRated.length) {
      <div class="mt-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <h3 class="text-sm font-semibold text-slate-700">Top-rated firms (national)</h3>
        <p class="mb-3 text-xs text-slate-500">
          Ranked by star rating weighted by review volume. Firms with offices across several
          searched locations are grouped into one entry.
        </p>
        <ol class="space-y-2">
          @for (firm of report().topRated; track firm.name; let i = $index) {
            <li class="flex items-center justify-between gap-3 rounded-lg bg-slate-50 px-4 py-2">
              <span class="flex items-start gap-3">
                <span class="mt-0.5 w-5 text-right font-semibold text-slate-400">{{ i + 1 }}</span>
                <span>
                  <span class="font-medium text-slate-800">{{ firm.name }}</span>
                  <span class="block text-xs text-slate-500">
                    {{ firm.locations.join(', ') }}
                    @if (firm.regionCount > 1) {
                      <span class="ml-1 rounded-full bg-slate-200 px-1.5 py-0.5 text-[10px] font-medium text-slate-600">
                        {{ firm.regionCount }} regions
                      </span>
                    }
                  </span>
                </span>
              </span>
              <span class="shrink-0 text-sm font-medium text-amber-600">
                {{ rankStars(firm) }}
              </span>
            </li>
          }
        </ol>
      </div>
    }

    <!-- Directory -->
    <div class="mt-4 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <h3 class="border-b border-slate-100 px-5 py-3 text-sm font-semibold text-slate-700">
        Solicitor directory ({{ report().firms.length }})
      </h3>
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-slate-50 text-left text-xs text-slate-500 uppercase">
            <tr>
              <th class="px-5 py-2 font-medium">Firm</th>
              <th class="px-3 py-2 font-medium">Location</th>
              <th class="px-3 py-2 font-medium">Address</th>
              <th class="px-3 py-2 font-medium">Phone</th>
              <th class="px-3 py-2 font-medium">Contact</th>
              <th class="px-5 py-2 text-right font-medium">Rating</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            @for (firm of report().firms; track firm.name + firm.location + firm.address) {
              <tr class="align-top hover:bg-slate-50">
                <td class="px-5 py-2 font-medium text-slate-800">{{ firm.name }}</td>
                <td class="px-3 py-2 text-slate-600">{{ firm.location }}</td>
                <td class="px-3 py-2 text-slate-600">{{ firm.address }}</td>
                <td class="px-3 py-2 whitespace-nowrap text-slate-600">{{ firm.phone ?? '—' }}</td>
                <td class="px-3 py-2">
                  <span class="flex gap-2">
                    @if (firm.website) {
                      <a [href]="firm.website" target="_blank" rel="noopener"
                         class="text-indigo-600 hover:underline">Web</a>
                    }
                    @if (firm.email) {
                      <a [href]="firm.email" target="_blank" rel="noopener"
                         class="text-indigo-600 hover:underline">Enquire</a>
                    }
                  </span>
                </td>
                <td class="px-5 py-2 text-right whitespace-nowrap text-amber-600">
                  {{ firm.stars !== null ? stars(firm) : '—' }}
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `,
})
export class ReportView {
  readonly report = input.required<SearchReport>();

  readonly contactBars = computed(() => {
    const c = this.report().contactability;
    return [
      { label: 'Phone', percent: c.phonePercent },
      { label: 'Website', percent: c.websitePercent },
      { label: 'Email / enquiry', percent: c.emailPercent },
      { label: 'Reachable (any)', percent: c.reachablePercent },
    ];
  });

  stars(firm: SolicitorView): string {
    if (firm.stars === null) return '—';
    const reviews = firm.reviewCount?.toLocaleString() ?? '0';
    return `${firm.stars.toFixed(1)} ★ (${reviews})`;
  }

  rankStars(firm: RankedFirm): string {
    return `${firm.stars.toFixed(1)} ★ (${firm.reviewCount.toLocaleString()})`;
  }
}
