import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../environments/environment';
import { App } from './app';

describe('App', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('loads the default locations on init', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges(); // runs ngOnInit

    httpMock.expectOne(`${environment.apiBaseUrl}/locations`).flush(['London', 'Leeds']);

    expect(fixture.componentInstance.locations()).toEqual(['London', 'Leeds']);
  });

  it('renders the heading', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    httpMock.expectOne(`${environment.apiBaseUrl}/locations`).flush([]);

    const heading = fixture.nativeElement.querySelector('h1');
    expect(heading.textContent).toContain('Solicitor Lead Finder');
  });
});
