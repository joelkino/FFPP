async function Refresh()
{
    document.getElementById('tenantsDropdownButton').disabled=true;
    document.getElementById('tenantData').innerHTML='';
    const tenantJson = await GetTenants(true);

    var dropItems="";

    for (var i = 0; i < tenantJson.length; i++){
        dropItems+=`<li><a class="onclick-no-blue dropdown-item modal-bg-dark" data-tenant="${tenantJson[i].defaultDomainName}" onclick="SelectOption('tenantName',this.innerText,this.dataset.tenant)">${tenantJson[i].displayName}</a></li>`; 
    }
    document.getElementById('tenantData').innerHTML=dropItems;
    document.getElementById('tenantsDropdownButton').disabled=false;
}

function DropdownButtonClick(dropdownId,searchInputId) {
    document.getElementById(searchInputId).value='';
    DropdownFilterFunction(dropdownId, searchInputId);
}

function SelectOption(dropdownButtonId, option='', value='')
{
    document.getElementById(dropdownButtonId).innerText=option.replace('*All','All').replace('* All','All');
    document.getElementById(dropdownButtonId).dataset.tenant=value;
}
  
function DropdownFilterFunction(dropdownId, searchInputId) {
    var input, filter, ul, li, a, i;
    input = document.getElementById(searchInputId);
    filter = input.value.toUpperCase();
    div = document.getElementById(dropdownId);
    a = div.getElementsByTagName("a");
    for (i = 0; i < a.length; i++) {
      txtValue = a[i].textContent || a[i].innerText;
      if (txtValue.toUpperCase().indexOf(filter) > -1) {
        a[i].style.display = "";
      } else {
        a[i].style.display = "none";
      }
    }
}