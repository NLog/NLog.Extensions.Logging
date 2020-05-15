npm install @prantlf/jsonlint -g

function validate-item($item) {

    $shortname = $item.name;
	Write-Output "Validate $shortname..."
	jsonlint $item.FullName --quiet --no-duplicate-keys --validate nlog.schema.json --environment json-schema-draft-07 2>&1
	if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }
}

$items = gci "examples";

foreach ($item in $items) {
   validate-item $item
}

Write-Output "JSON Validation done"