async function FetchUrl(url, method = "GET")
{
  async function RepeatableFetch(url, method)
  {
    var signInData = await SignIn();
    let responsePayload = await fetch(url,
    {
      method: method,
      credentials: 'include',
      headers: {
        'Authorization': 'Bearer '+signInData.accessToken
      }
    });
    return await responsePayload.json();
  }

  try {
      return await RepeatableFetch(url, method);
  }
  catch (e)
  {
      return await RepeatableFetch(url, method);
  }
}

async function AuthMe() {
   FetchUrl('/.auth/me').then( function(response)
  {return response}).catch(function(error){console.error(error);});
}

async function GetTenants(allTenantSelector = false)
{
  try
  {
    var response = await FetchUrl(`/api/ListTenants?AllTenantSelector=${allTenantSelector}`);
    return response;
  }
  catch(error)
  {
    console.error(error);
  }

}
