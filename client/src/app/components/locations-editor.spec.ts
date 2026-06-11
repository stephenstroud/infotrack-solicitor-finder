import { TestBed } from '@angular/core/testing';
import { LocationsEditor } from './locations-editor';

describe('LocationsEditor', () => {
  function create(initial: string[]) {
    const fixture = TestBed.createComponent(LocationsEditor);
    fixture.componentRef.setInput('locations', initial);
    return fixture.componentInstance;
  }

  it('adds a trimmed location and ignores case-insensitive duplicates', () => {
    const editor = create(['London']);

    editor.draft.set('  Leeds  ');
    editor.add();
    expect(editor.locations()).toEqual(['London', 'Leeds']);

    editor.draft.set('leeds');
    editor.add();
    expect(editor.locations()).toEqual(['London', 'Leeds']);
  });

  it('does not add blank input', () => {
    const editor = create([]);
    editor.draft.set('   ');
    editor.add();
    expect(editor.locations()).toEqual([]);
  });

  it('removes a location', () => {
    const editor = create(['London', 'Leeds']);
    editor.remove('London');
    expect(editor.locations()).toEqual(['Leeds']);
  });
});
