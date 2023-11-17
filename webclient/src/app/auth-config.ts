import {LogLevel, Configuration, BrowserCacheLocation} from '@azure/msal-browser';
import {environment} from '../environments/environment';

export const msalConfig: Configuration = {
  auth: {
    clientId: environment.azureClientId,
    authority: `https://login.microsoftonline.com/${environment.azureTenantId}`,
    redirectUri: environment.appRootUrl,
  },
  cache: {
    cacheLocation: BrowserCacheLocation.LocalStorage,
  },
  system: {
    loggerOptions: {
      loggerCallback: (logLevel, message) => {
        console.log(message);
      },
      logLevel: LogLevel.Warning,
      piiLoggingEnabled: false
    }
  }
};

export const protectedResources = {
  webio: new Map([
    [`${environment.appRootUrl}/api`, ['api://webio-api/Read']],
  ])
};
