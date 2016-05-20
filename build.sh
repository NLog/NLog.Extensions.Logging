#!/bin/bash

dotnet restore src/NLog.Extensions.Logging
dotnet build --configuration release src/NLog.Extensions.Logging
