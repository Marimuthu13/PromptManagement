$baseUrl = "http://localhost:5256/api"

function Invoke-Api {
    param($method, $url, $body, $token)
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    if ($body) { $headers["Content-Type"] = "application/json" }
    
    try {
        if ($method -eq "GET" -or $method -eq "DELETE") {
            $res = Invoke-WebRequest -Uri $url -Method $method -Headers $headers -UseBasicParsing
        } else {
            $res = Invoke-WebRequest -Uri $url -Method $method -Headers $headers -Body $body -UseBasicParsing
        }
        return [int]$res.StatusCode, $res.Content
    } catch {
        return [int]$_.Exception.Response.StatusCode, $_.Exception.Response.Content
    }
}

# Create User A
$regA = '{"username":"UserA", "email":"usera' + (Get-Random) + '@test.com", "password":"password123"}'
Invoke-Api "POST" "$baseUrl/Auth/register" $regA $null | Out-Null
$loginPayloadA = $regA | ConvertFrom-Json | Select-Object email, password | ConvertTo-Json -Compress
$tokenA = (Invoke-Api "POST" "$baseUrl/Auth/login" $loginPayloadA $null)[1] | ConvertFrom-Json | Select-Object -ExpandProperty accessToken

Write-Host "--- AUTH CHECKS ---"
Write-Host "Unauth POST -> 401: " (Invoke-Api "POST" "$baseUrl/categories" "{}" $null)[0]
Write-Host "Unauth PUT -> 401: " (Invoke-Api "PUT" "$baseUrl/categories/00000000-0000-0000-0000-000000000000" "{}" $null)[0]
Write-Host "Unauth DELETE -> 401: " (Invoke-Api "DELETE" "$baseUrl/categories/00000000-0000-0000-0000-000000000000" $null $null)[0]
Write-Host "Unauth GET List -> 200: " (Invoke-Api "GET" "$baseUrl/categories" $null $null)[0]

Write-Host "--- CREATE CHECKS ---"
$cat1 = "{ `"name`": `"Cat $(Get-Random)`", `"description`": `"Test`" }"
$cat1Res = Invoke-Api "POST" "$baseUrl/categories" $cat1 $tokenA
Write-Host "Valid POST -> 201: " $cat1Res[0]
$catId = $cat1Res[1] | ConvertFrom-Json | Select-Object -ExpandProperty id

$dupRes = Invoke-Api "POST" "$baseUrl/categories" $cat1 $tokenA
Write-Host "Duplicate POST -> 409: " $dupRes[0]
Write-Host "Missing Name POST -> 400: " (Invoke-Api "POST" "$baseUrl/categories" "{ `"name`": `"`" }" $tokenA)[0]

Write-Host "--- GET CHECKS ---"
Write-Host "GET By Id -> 200: " (Invoke-Api "GET" "$baseUrl/categories/$catId" $null $null)[0]
Write-Host "GET By Id Missing -> 404: " (Invoke-Api "GET" "$baseUrl/categories/00000000-0000-0000-0000-000000000000" $null $null)[0]

Write-Host "--- UPDATE CHECKS ---"
$update1 = "{ `"id`": `"$catId`", `"name`": `"Cat Updated $(Get-Random)`", `"description`": `"Test Update`" }"
Write-Host "Valid PUT -> 200: " (Invoke-Api "PUT" "$baseUrl/categories/$catId" $update1 $tokenA)[0]
Write-Host "PUT Missing -> 404: " (Invoke-Api "PUT" "$baseUrl/categories/00000000-0000-0000-0000-000000000000" "{ `"id`": `"00000000-0000-0000-0000-000000000000`", `"name`": `"x`", `"description`": `"y`" }" $tokenA)[0]

Write-Host "--- CONFLICT DELETE CHECKS ---"
$promptA = "{ `"title`": `"Prompt A`", `"content`": `"Content A`", `"categoryId`": `"$catId`" }"
Invoke-Api "POST" "$baseUrl/prompts" $promptA $tokenA | Out-Null
Write-Host "DELETE referenced category -> 409: " (Invoke-Api "DELETE" "$baseUrl/categories/$catId" $null $tokenA)[0]

Write-Host "--- DELETE CHECKS ---"
$cat2 = "{ `"name`": `"Cat $(Get-Random)`", `"description`": `"Test 2`" }"
$cat2Res = Invoke-Api "POST" "$baseUrl/categories" $cat2 $tokenA
$cat2Id = $cat2Res[1] | ConvertFrom-Json | Select-Object -ExpandProperty id
Write-Host "DELETE unreferenced category -> 204: " (Invoke-Api "DELETE" "$baseUrl/categories/$cat2Id" $null $tokenA)[0]
Write-Host "DELETE missing -> 404: " (Invoke-Api "DELETE" "$baseUrl/categories/$cat2Id" $null $tokenA)[0]
