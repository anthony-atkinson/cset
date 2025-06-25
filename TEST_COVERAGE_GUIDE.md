# Test Coverage Guide

## Overview

This document provides comprehensive information about test coverage implementation in the CSET project, including setup, usage, and improvement guidelines.

## 📊 Coverage Implementation

### Backend Coverage (.NET)

The backend uses **coverlet.collector** for code coverage analysis with the following configuration:

#### Coverage Tools
- **Coverlet.Collector**: Code coverage collection for .NET
- **ReportGenerator**: Coverage report generation
- **XPlat Code Coverage**: Cross-platform coverage collection

#### Configuration Files
- `CSETWebApi/CSETWeb_Api/coverage-config.json` - Coverage configuration
- `.github/workflows/test-coverage.yml` - CI/CD integration

#### Coverage Thresholds
- **Statements**: 70%
- **Branches**: 60%
- **Functions**: 70%
- **Lines**: 70%

#### Exclusions
The following files/directories are excluded from coverage:
- Program.cs and Startup.cs files
- Migration files
- AutoMapper configuration
- Test files (*.Test.cs, *.Tests.cs)
- Mock/Fake/Stub files
- Generated code directories (obj/, bin/)
- Health and Error controllers

### Frontend Coverage (Angular)

The frontend uses **karma-coverage-istanbul-reporter** for code coverage analysis:

#### Coverage Tools
- **karma-coverage-istanbul-reporter**: Coverage collection and reporting
- **Istanbul**: JavaScript code coverage tool
- **Karma**: Test runner integration

#### Configuration Files
- `CSETWebNg/karma.conf.js` - Karma configuration with coverage
- `CSETWebNg/coverage-config.json` - Coverage configuration
- `CSETWebNg/package.json` - Test scripts

#### Coverage Thresholds
- **Statements**: 70%
- **Branches**: 60%
- **Functions**: 70%
- **Lines**: 70%

#### Exclusions
The following files/directories are excluded from coverage:
- Test files (*.spec.ts, *.test.ts)
- Mock files (*.mock.ts)
- Module files (*.module.ts)
- Environment files
- Main entry points (main.ts, polyfills.ts)
- Type definition files (*.d.ts)

## 🚀 Running Coverage Locally

### Backend Coverage

```bash
# Navigate to the API directory
cd CSETWebApi/CSETWeb_Api

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" \
  --results-directory ./coverage \
  --logger trx \
  --verbosity normal \
  --configuration Release

# Generate coverage report
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:./coverage/*/cobertura.xml \
  -targetdir:./coverage/report \
  -reporttypes:Html;Cobertura;JsonSummary \
  -verbosity:Info
```

### Frontend Coverage

```bash
# Navigate to the Angular directory
cd CSETWebNg

# Run tests with coverage
npm run test:coverage
```

## 📈 Coverage Reports

### Report Locations
- **Backend**: `CSETWebApi/CSETWeb_Api/coverage/report/`
- **Frontend**: `CSETWebNg/coverage/`

### Report Formats
- **HTML**: Interactive coverage reports
- **LCOV**: Coverage data for CI/CD integration
- **JSON Summary**: Coverage statistics for automation
- **Text Summary**: Console output with coverage summary

### Accessing Reports
1. **Local Development**: Open `index.html` in the coverage report directory
2. **CI/CD**: Reports are available as build artifacts
3. **GitHub Actions**: Coverage summary is posted in pull requests

## 🔧 CI/CD Integration

### GitHub Actions Workflow
The coverage reporting is integrated into the CI/CD pipeline via `.github/workflows/test-coverage.yml`:

#### Triggers
- Pull requests to develop and release branches
- Pushes to develop and release branches
- Manual workflow dispatch

#### Jobs
1. **Backend Coverage**: Runs .NET tests with coverage collection
2. **Frontend Coverage**: Runs Angular tests with coverage collection
3. **Coverage Summary**: Generates summary report and posts to PR

#### Artifacts
- Coverage reports are uploaded as build artifacts
- Reports are retained for 30 days
- Coverage data is available for trend analysis

### Coverage Thresholds
- Coverage thresholds are enforced in CI/CD
- Builds fail if coverage drops below thresholds
- Coverage summary is posted in pull requests

## 📋 Coverage Improvement Guidelines

### General Guidelines

1. **Write Tests First**: Follow TDD practices when possible
2. **Focus on Business Logic**: Prioritize coverage for business-critical code
3. **Test Edge Cases**: Ensure error conditions and boundary cases are covered
4. **Maintain Test Quality**: Keep tests readable, maintainable, and fast

### Backend Guidelines

1. **Controller Coverage**: Aim for 90%+ coverage on API controllers
2. **Business Logic**: Ensure all business logic classes have comprehensive tests
3. **Data Access**: Mock external dependencies, test data access logic
4. **Error Handling**: Test error conditions and exception handling

### Frontend Guidelines

1. **Component Coverage**: Test component logic and user interactions
2. **Service Coverage**: Ensure all services have comprehensive tests
3. **Pipe Coverage**: Test custom pipes and transformations
4. **Guard Coverage**: Test route guards and authentication logic

### Coverage Exclusions

#### When to Exclude Code
- Generated code (migrations, auto-generated files)
- Configuration files
- Entry points (Program.cs, main.ts)
- Test utilities and mocks
- Third-party library code

#### Exclusion Guidelines
- Document all exclusions with clear reasoning
- Review exclusions regularly
- Avoid excluding business logic
- Use specific patterns rather than broad exclusions

## 📊 Coverage Metrics

### Key Metrics
- **Line Coverage**: Percentage of lines executed
- **Branch Coverage**: Percentage of branches taken
- **Function Coverage**: Percentage of functions called
- **Statement Coverage**: Percentage of statements executed

### Coverage Goals
- **Minimum**: 70% overall coverage
- **Target**: 80% overall coverage
- **Excellent**: 90%+ overall coverage

### Coverage Trends
- Monitor coverage trends over time
- Set up alerts for coverage drops
- Track coverage by component/module
- Identify areas needing improvement

## 🛠️ Troubleshooting

### Common Issues

#### Backend Coverage Issues
1. **No Coverage Data**: Ensure coverlet.collector is installed
2. **Missing Reports**: Check ReportGenerator installation
3. **Exclusion Issues**: Verify exclusion patterns in configuration

#### Frontend Coverage Issues
1. **No Coverage Data**: Check karma-coverage-istanbul-reporter installation
2. **Missing Reports**: Verify karma configuration
3. **Threshold Failures**: Review exclusion patterns

### Debugging Steps
1. Check package/project file configurations
2. Verify test execution and coverage collection
3. Review exclusion patterns and thresholds
4. Check CI/CD workflow configuration

## 📚 Additional Resources

### Documentation
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)
- [Karma Coverage Documentation](https://github.com/karma-runner/karma-coverage)
- [Istanbul Documentation](https://istanbul.js.org/)

### Best Practices
- [Microsoft .NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Angular Testing Best Practices](https://angular.io/guide/testing)
- [Test Coverage Best Practices](https://martinfowler.com/bliki/TestCoverage.html)

## 🔄 Maintenance

### Regular Tasks
1. **Review Coverage Reports**: Monthly review of coverage trends
2. **Update Exclusions**: Quarterly review of exclusion patterns
3. **Adjust Thresholds**: Annual review of coverage thresholds
4. **Update Tools**: Keep coverage tools up to date

### Continuous Improvement
1. **Identify Gaps**: Use coverage reports to identify untested code
2. **Prioritize Testing**: Focus on high-impact, low-coverage areas
3. **Refactor Tests**: Improve test quality and maintainability
4. **Educate Team**: Share testing best practices and guidelines

---

## 📞 Support

For questions or issues related to test coverage:
1. Check this documentation first
2. Review the troubleshooting section
3. Check GitHub Issues for known problems
4. Contact the development team

---

*Last updated: $(date)* 