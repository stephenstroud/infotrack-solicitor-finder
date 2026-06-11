/** Mirrors the API's SearchReport / SolicitorView contracts (camel-cased by the serializer). */

export interface SolicitorView {
  name: string;
  location: string;
  address: string;
  city: string | null;
  postcode: string | null;
  phone: string | null;
  email: string | null;
  website: string | null;
  description: string | null;
  channelCount: number;
  isReachable: boolean;
  stars: number | null;
  reviewCount: number | null;
}

export interface LocationBreakdown {
  location: string;
  firmCount: number;
  reachableFirms: number;
  withPhone: number;
  withEmail: number;
  withWebsite: number;
  reachablePercent: number;
}

export interface ContactabilityStats {
  totalFirms: number;
  withPhone: number;
  withEmail: number;
  withWebsite: number;
  reachable: number;
  phonePercent: number;
  emailPercent: number;
  websitePercent: number;
  reachablePercent: number;
}

export interface SearchReport {
  snapshotId: string;
  generatedAt: string;
  totalFirms: number;
  locationsSearched: number;
  contactability: ContactabilityStats;
  breakdown: LocationBreakdown[];
  topRated: SolicitorView[];
  newFirms: SolicitorView[];
  firms: SolicitorView[];
  topLocation: string | null;
  hasNewFirms: boolean;
}
