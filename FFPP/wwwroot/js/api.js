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

async function GetTenants(allTenantSelector = false)
{

}
