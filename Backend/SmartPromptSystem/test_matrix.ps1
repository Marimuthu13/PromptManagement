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

Write-Host "No JWT -> GET prompts: " (Invoke-Api "GET" "$baseUrl/prompts" $null $null)[0]
Write-Host "No JWT -> GET prompt by ID: " (Invoke-Api "GET" "$baseUrl/prompts/00000000-0000-0000-0000-000000000000" $null $null)[0]
Write-Host "No JWT -> PUT prompt: " (Invoke-Api "PUT" "$baseUrl/prompts/00000000-0000-0000-0000-000000000000" "{}" $null)[0]
Write-Host "No JWT -> DELETE prompt: " (Invoke-Api "DELETE" "$baseUrl/prompts/00000000-0000-0000-0000-000000000000" $null $null)[0]

# Create User A
$regA = '{"username":"UserA", "email":"usera' + (Get-Random) + '@test.com", "password":"password123"}'
Invoke-Api "POST" "$baseUrl/Auth/register" $regA $null | Out-Null
$loginPayloadA = $regA | ConvertFrom-Json | Select-Object email, password | ConvertTo-Json -Compress
$tokenA = (Invoke-Api "POST" "$baseUrl/Auth/login" $loginPayloadA $null)[1] | ConvertFrom-Json | Select-Object -ExpandProperty accessToken

# Create User B
$regB = '{"username":"UserB", "email":"userb' + (Get-Random) + '@test.com", "password":"password123"}'
Invoke-Api "POST" "$baseUrl/Auth/register" $regB $null | Out-Null
$loginPayloadB = $regB | ConvertFrom-Json | Select-Object email, password | ConvertTo-Json -Compress
$tokenB = (Invoke-Api "POST" "$baseUrl/Auth/login" $loginPayloadB $null)[1] | ConvertFrom-Json | Select-Object -ExpandProperty accessToken

# Create category
$cat = '{"name":"Test Category ' + (Get-Random) + '", "description":"Test"}'
$catResponse = (Invoke-Api "POST" "$baseUrl/categories" $cat $tokenA)
$catId = $catResponse[1] | ConvertFrom-Json | Select-Object -ExpandProperty id

# Create Prompt for A
$promptA = "{ `"title`":`"Prompt A`", `"content`":`"Content A`", `"categoryId`":`"$catId`" }"
$promptAResponse = (Invoke-Api "POST" "$baseUrl/prompts" $promptA $tokenA)
$promptAId = $promptAResponse[1] | ConvertFrom-Json | Select-Object -ExpandProperty id

# Create Prompt for B
$promptB = "{ `"title`":`"Prompt B`", `"content`":`"Content B`", `"categoryId`":`"$catId`" }"
$promptBResponse = (Invoke-Api "POST" "$baseUrl/prompts" $promptB $tokenB)
$promptBId = $promptBResponse[1] | ConvertFrom-Json | Select-Object -ExpandProperty id

Write-Host "A gets A's prompt: " (Invoke-Api "GET" "$baseUrl/prompts/$promptAId" $null $tokenA)[0]
Write-Host "A gets B's prompt: " (Invoke-Api "GET" "$baseUrl/prompts/$promptBId" $null $tokenA)[0]

$updateA = "{ `"id`":`"$promptAId`", `"title`":`"Updated A`", `"content`":`"Content`", `"categoryId`":`"$catId`" }"
$updateB = "{ `"id`":`"$promptBId`", `"title`":`"Updated B`", `"content`":`"Content`", `"categoryId`":`"$catId`" }"
Write-Host "A updates A's prompt: " (Invoke-Api "PUT" "$baseUrl/prompts/$promptAId" $updateA $tokenA)[0]
Write-Host "A updates B's prompt: " (Invoke-Api "PUT" "$baseUrl/prompts/$promptBId" $updateB $tokenA)[0]

Write-Host "A deletes B's prompt: " (Invoke-Api "DELETE" "$baseUrl/prompts/$promptBId" $null $tokenA)[0]
Write-Host "A deletes A's prompt: " (Invoke-Api "DELETE" "$baseUrl/prompts/$promptAId" $null $tokenA)[0]

Write-Host "B gets B's prompt: " (Invoke-Api "GET" "$baseUrl/prompts/$promptBId" $null $tokenB)[0]

$emptyTitle = "{ `"id`":`"$promptBId`", `"title`":`"`", `"content`":`"Content`", `"categoryId`":`"$catId`" }"
$emptyContent = "{ `"id`":`"$promptBId`", `"title`":`"T`", `"content`":`"`", `"categoryId`":`"$catId`" }"
$invalidCat = "{ `"id`":`"$promptBId`", `"title`":`"T`", `"content`":`"C`", `"categoryId`":`"00000000-0000-0000-0000-000000000000`" }"

Write-Host "Empty title -> 400: " (Invoke-Api "PUT" "$baseUrl/prompts/$promptBId" $emptyTitle $tokenB)[0]
Write-Host "Empty content -> 400: " (Invoke-Api "PUT" "$baseUrl/prompts/$promptBId" $emptyContent $tokenB)[0]
Write-Host "Invalid category -> 404: " (Invoke-Api "PUT" "$baseUrl/prompts/$promptBId" $invalidCat $tokenB)[0]

Write-Host "Search restricted to A: " (Invoke-Api "GET" "$baseUrl/prompts?search=Prompt" $null $tokenA)[1]
