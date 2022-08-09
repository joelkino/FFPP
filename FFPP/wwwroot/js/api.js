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
  try
  {
    return await FetchUrl('/.auth/me');
  }
  catch(error)
  {
    console.error(error);
  }

}

async function GetTenants(allTenantSelector = false)
{
  try
  {
    return await FetchUrl(`/api/ListTenants?AllTenantSelector=${allTenantSelector}`);
  }
  catch(error)
  {
    console.error(error);
  }

}
