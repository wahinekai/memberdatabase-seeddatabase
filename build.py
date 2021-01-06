#!/usr/bin/env python3

from optparse import OptionParser
from os import system

# Globals
sln = "./src/SeedDatabase.sln"
project = "./src/SeedDatabaseHost/SeedDatabaseHost.csproj"

def parse_command_line_arguments():
    parser = OptionParser()
    parser.add_option("-c", "--clean", action="store_true", dest="clean", default=False, help="Clean the project")
    parser.add_option("-r", "--restore", action="store_true", dest="restore", default=False, help="Restore project dependencies")
    parser.add_option("-b", "--build", action="store_true", dest="build", default=False, help="Build Project")
    parser.add_option("-t", "--test", action="store_true", dest="test", default=False, help="Test Project")
    parser.add_option("-R", "--run", action="store_true", dest="run", default=False, help="Run Project")
    parser.add_option("-o", "--output", dest="output", default="./out", help="Project output directory, defaults to ./out")
    parser.add_option("-C", "--configuration", dest="configuration", default="Debug", help="Project configuration type, defaults to Debug")
    return parser.parse_args()

def dotnetRestore():
    print("Restoring Project Dependencies")
    system(f"dotnet restore {sln}")

def dotnetBuild(configuration, output):
    print("Building project")
    system(f"dotnet build {sln} --no-restore --configuration {configuration} -o {output}")

def dotnetClean(output):
    print("Cleaning project")
    system(f"rm -rf {output}")
    system(f"dotnet clean {sln}")

def dotnetTest(configuration, output):
    print("Testing project")
    system(f"dotnet test {sln} --no-build --configuration {configuration} /p:OutputPath={output}")

def dotnetRun(configuration, output):
    print("Testing project")
    system(f"dotnet run {sln} --no-build --project {project} --configuration {configuration} /p:OutputPath={output}")

def main():
    print("Parsing command-line arguments")
    (options, _args) = parse_command_line_arguments()

    if options.clean:
        dotnetClean(options.output)

    if options.restore:
        dotnetRestore()

    if options.build:
        dotnetBuild(options.configuration, options.output)

    if options.test:
        dotnetTest(options.configuration, options.output)

    if options.run:
        dotnetRun(options.configuration, options.output)

if __name__ == "__main__":
    main()