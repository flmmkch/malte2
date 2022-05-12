/***************************************************************************************************
 * Load `$localize` onto the global scope - used if i18n tags appear in Angular templates.
 */
import '@angular/localize/init';
import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

export function getApiBaseUrl() {
  return document.querySelector('meta[name="Malte2api"]')?.getAttribute('content');
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  { provide: 'ASSETS_URL', useFactory: () => `${getBaseUrl()}assets`, deps: [] },
  { provide: 'API_BASE_URL', useFactory: getApiBaseUrl, deps: [] },
  { provide: 'APP_VERSION', useValue: '0.4' },
];

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.log(err));
