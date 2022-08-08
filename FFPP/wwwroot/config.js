/* Don't put secret configuration settings in this file, this is rendered
by the client. */

const config = {
  auth: {
    clientId: '064e4ec3-5dcd-410e-959c-2bfd3e47c0d5',
    authority: 'https://login.microsoftonline.com/organizations/',
    redirectUri: '/home/index.html'
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false
  },
  api: {
    scopes: ['https://whiteknightit.onmicrosoft.com/df80cb54-6ed0-4c6b-9a31-a3e038232451/cipp-api-alt.access']
  }
};
