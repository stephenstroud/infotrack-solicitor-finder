import { Component, model, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

/**
 * Editable list of locations as removable chips plus an add box.
 * Two-way bound to the parent via a `model()` signal — the parent owns the list.
 */
@Component({
  selector: 'app-locations-editor',
  imports: [FormsModule],
  template: `
    <div class="flex flex-wrap items-center gap-2">
      @for (location of locations(); track location) {
        <span
          class="inline-flex items-center gap-1 rounded-full bg-indigo-100 py-1 pr-1 pl-3 text-sm font-medium text-indigo-800"
        >
          {{ location }}
          <button
            type="button"
            (click)="remove(location)"
            [attr.aria-label]="'Remove ' + location"
            class="flex h-5 w-5 items-center justify-center rounded-full text-indigo-500 hover:bg-indigo-200 hover:text-indigo-900"
          >
            &times;
          </button>
        </span>
      } @empty {
        <span class="text-sm text-slate-400">No locations yet — add one to get started.</span>
      }
    </div>

    <div class="mt-3 flex gap-2">
      <input
        [(ngModel)]="draft"
        (keyup.enter)="add()"
        type="text"
        placeholder="Add a location (e.g. Nottingham)"
        class="w-64 rounded-md border border-slate-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 focus:outline-none"
      />
      <button
        type="button"
        (click)="add()"
        class="rounded-md bg-slate-200 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-300"
      >
        Add
      </button>
    </div>
  `,
})
export class LocationsEditor {
  /** The location list, two-way bound to the parent. */
  readonly locations = model<string[]>([]);
  readonly draft = signal('');

  add(): void {
    const value = this.draft().trim();
    if (!value) return;

    const alreadyPresent = this.locations().some((l) => l.toLowerCase() === value.toLowerCase());
    if (!alreadyPresent) {
      this.locations.set([...this.locations(), value]);
    }
    this.draft.set('');
  }

  remove(location: string): void {
    this.locations.set(this.locations().filter((l) => l !== location));
  }
}
