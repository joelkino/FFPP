/* Don't put secret configuration settings in this file, this is rendered
by the client. */

const config = {
  auth: {
    clientId: '9740fb35-7c94-4386-940e-56587f0172d5',
    authority: 'https://login.microsoftonline.com/organizations/',
    redirectUri: '/index.html'
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false
  },
  api: {
    scopes: ['https://whiteknightit.onmicrosoft.com/ca6a657e-7317-447d-99a9-b36ec9a40cfa/ffpp-api.access']
  }
};
