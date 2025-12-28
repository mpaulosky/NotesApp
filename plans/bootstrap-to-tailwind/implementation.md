# Bootstrap to Tailwind CSS Conversion - Implementation Guide

## Goal

Convert all Bootstrap-style classes to Tailwind CSS utilities across the Notes.Web Blazor application, replacing custom scoped CSS with Tailwind component classes and inline SVG icons.

## Prerequisites

Make sure you are currently on the `wip/update-to-tailwindcss` branch before beginning implementation.

```powershell
# Check current branch
git branch --show-current

# If not on correct branch, switch to it
git checkout wip/update-to-tailwindcss

# If branch doesn't exist, create it from main
git checkout -b wip/update-to-tailwindcss main
```

---

## Step-by-Step Instructions

### Step 1: Convert Counter Page Button

Update the Counter page to use Tailwind button classes.

- [ ] Open `src/Notes.Web/Components/Pages/Counter.razor`
- [ ] Replace the button element with Tailwind classes:

```razor
@page "/counter"
@rendermode InteractiveServer

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @currentCount</p>

<button class="bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded transition-colors" @onclick="IncrementCount">Click me</button>

@code {
	private int currentCount = 0;

	private void IncrementCount()
	{
		currentCount++;
	}
}
```

#### Step 1 Verification Checklist

- [ ] Run `dotnet build` - no errors
- [ ] Navigate to `/counter` page
- [ ] Verify button has blue background
- [ ] Verify button darkens on hover
- [ ] Verify button text is white
- [ ] Click button and confirm counter increments

#### Step 1 STOP & COMMIT

**STOP & COMMIT:** Test the changes, then stage and commit:

```powershell
git add src/Notes.Web/Components/Pages/Counter.razor
git commit -m "Convert Counter page button to Tailwind classes"
```

---

### Step 2: Convert Error Page Text

Update the Error page to use Tailwind text color utilities.

- [ ] Open `src/Notes.Web/Components/Pages/Error.razor`
- [ ] Replace `text-danger` classes with Tailwind's `text-red-600`:

```razor
@page "/Error"
@using System.Diagnostics

<PageTitle>Error</PageTitle>

<h1 class="text-red-600">Error.</h1>
<h2 class="text-red-600">An error occurred while processing your request.</h2>

@if (ShowRequestId)
{
	<p>
		<strong>Request ID:</strong> <code>@requestId</code>
	</p>
}

<h3>Development Mode</h3>
<p>
	Swapping to <strong>Development</strong> environment will display more detailed information about the error that occurred.
</p>
<p>
	<strong>The Development environment shouldn't be enabled for deployed applications.</strong>
	It can result in displaying sensitive information from exceptions to end users.
	For local debugging, enable the <strong>Development</strong> environment by setting the <strong>ASPNETCORE_ENVIRONMENT</strong> environment variable to <strong>Development</strong>
	and restarting the app.
</p>

@code{
	[CascadingParameter] public HttpContext? HttpContext { get; set; }

	private string? requestId;

	private bool ShowRequestId => !string.IsNullOrEmpty(requestId);

	protected override void OnInitialized()
	{
		requestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
	}
}
```

#### Step 2 Verification Checklist

- [ ] Run `dotnet build` - no errors
- [ ] Navigate to `/Error` page
- [ ] Verify both headings display in red color (rgb(220, 38, 38) / #dc2626)

#### Step 2 STOP & COMMIT

**STOP & COMMIT:** Test the changes, then stage and commit:

```powershell
git add src/Notes.Web/Components/Pages/Error.razor
git commit -m "Convert Error page to Tailwind text utilities"
```

---

### Step 3: Convert Weather Page Table

Update the Weather page table with Tailwind table utilities.

- [ ] Open `src/Notes.Web/Components/Pages/Weather.razor`
- [ ] Replace table markup with Tailwind classes:

```razor
@page "/weather"
@attribute [StreamRendering(true)]
@attribute [OutputCache(Duration = 5)]

@inject WeatherApiClient WeatherApi

<PageTitle>Weather</PageTitle>

<h1>Weather</h1>

<p>This component demonstrates showing data loaded from a backend API service.</p>

@if (forecasts == null)
{
	<p>
		<em>Loading...</em>
	</p>
}
else
{
	<table class="min-w-full divide-y divide-gray-200">
		<thead class="bg-gray-50">
		<tr>
			<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
			<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" aria-label="Temperature in Celsius">Temp. (C)</th>
			<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" aria-label="Temperature in Fahrenheit">Temp. (F)</th>
			<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Summary</th>
		</tr>
		</thead>
		<tbody class="bg-white divide-y divide-gray-200">
		@foreach (var forecast in forecasts)
		{
			<tr>
				<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">@forecast.Date.ToShortDateString()</td>
				<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">@forecast.TemperatureC</td>
				<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">@forecast.TemperatureF</td>
				<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">@forecast.Summary</td>
			</tr>
		}
		</tbody>
	</table>
}

@code {
	private WeatherForecast[]? forecasts;

	protected override async Task OnInitializedAsync()
	{
		forecasts = await WeatherApi.GetWeatherAsync();
	}
}
```

#### Step 3 Verification Checklist

- [ ] Run `dotnet build` - no errors
- [ ] Navigate to `/weather` page
- [ ] Verify table has gray header background
- [ ] Verify table has horizontal divider lines between rows
- [ ] Verify proper spacing in table cells
- [ ] Verify text is properly aligned left
- [ ] Test responsive behavior (table should scroll horizontally on small screens)

#### Step 3 STOP & COMMIT

**STOP & COMMIT:** Test the changes, then stage and commit:

```powershell
git add src/Notes.Web/Components/Pages/Weather.razor
git commit -m "Convert Weather page table to Tailwind utilities"
```

---

### Step 4: Convert MainLayout Structure

Replace MainLayout.razor.css with Tailwind utilities and component classes.

- [ ] Open `src/Notes.Web/Components/Layout/MainLayout.razor`
- [ ] Update the layout markup (no changes needed - already uses Tailwind `px-4`):

```razor
@inherits LayoutComponentBase

<div class="page">
	<div class="sidebar">
		<NavMenu/>
	</div>

	<main>
		<div class="top-row px-4">
			<a href="https://learn.microsoft.com/aspnet/core/" target="_blank">About</a>
		</div>

		<article class="content px-4">
			@Body
		</article>
	</main>
</div>

<div id="blazor-error-ui">
	An unhandled error has occurred.
	<a href="" class="reload">Reload</a>
	<a class="dismiss">🗙</a>
</div>
```

- [ ] Delete `src/Notes.Web/Components/Layout/MainLayout.razor.css`

```powershell
Remove-Item src/Notes.Web/Components/Layout/MainLayout.razor.css
```

#### Step 4 Verification Checklist

- [ ] Run `dotnet build` - no errors
- [ ] Verify MainLayout.razor.css file is deleted
- [ ] Navigate to home page (`/`)
- [ ] Verify layout structure appears (sidebar on left, main content on right on desktop)
- [ ] Verify gradient sidebar background (dark blue to purple)
- [ ] Test responsive behavior - resize browser to mobile width
- [ ] Verify sidebar moves to top on mobile
- [ ] Verify "About" link in top-row is visible

**Note:** Complete styling will be applied in Step 6 via app.css component classes.

#### Step 4 STOP & COMMIT

**STOP & COMMIT:** Test the changes, then stage and commit:

```powershell
git add src/Notes.Web/Components/Layout/MainLayout.razor
git add src/Notes.Web/Components/Layout/MainLayout.razor.css
git commit -m "Remove MainLayout.razor.css scoped styles"
```

---

### Step 5: Convert NavMenu with Inline SVG Icons

Replace Bootstrap icon spans with inline SVG elements and remove scoped CSS.

- [ ] Open `src/Notes.Web/Components/Layout/NavMenu.razor`
- [ ] Replace entire content with Tailwind classes and inline SVG:

```razor
<div class="top-row ps-3 min-h-14 bg-black/40">
	<div class="w-full">
		<a class="navbar-brand" href="">AspireApp1</a>
	</div>
</div>

<input type="checkbox" title="Navigation menu" class="navbar-toggler"/>

<div class="nav-scrollable" onclick="document.querySelector('.navbar-toggler').click()">
	<nav class="flex flex-col">
		<div class="nav-item px-3">
			<NavLink class="nav-link" href="" Match="NavLinkMatch.All">
				<svg class="inline-block w-5 h-5 mr-3 -mt-px" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 16 16" aria-hidden="true">
					<path d="M6.5 14.5v-3.505c0-.245.25-.495.5-.495h2c.25 0 .5.25.5.5v3.5a.5.5 0 0 0 .5.5h4a.5.5 0 0 0 .5-.5v-7a.5.5 0 0 0-.146-.354L13 5.793V2.5a.5.5 0 0 0-.5-.5h-1a.5.5 0 0 0-.5.5v1.293L8.354 1.146a.5.5 0 0 0-.708 0l-6 6A.5.5 0 0 0 1.5 7.5v7a.5.5 0 0 0 .5.5h4a.5.5 0 0 0 .5-.5Z"/>
				</svg>
				Home
			</NavLink>
		</div>

		<div class="nav-item px-3">
			<NavLink class="nav-link" href="counter">
				<svg class="inline-block w-5 h-5 mr-3 -mt-px" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 16 16" aria-hidden="true">
					<path d="M2 0a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V2a2 2 0 0 0-2-2H2zm6.5 4.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3a.5.5 0 0 1 1 0z"/>
				</svg>
				Counter
			</NavLink>
		</div>

		<div class="nav-item px-3">
			<NavLink class="nav-link" href="weather">
				<svg class="inline-block w-5 h-5 mr-3 -mt-px" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 16 16" aria-hidden="true">
					<path fill-rule="evenodd" d="M4.5 11.5A.5.5 0 0 1 5 11h10a.5.5 0 0 1 0 1H5a.5.5 0 0 1-.5-.5zm-2-4A.5.5 0 0 1 3 7h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5zm-2-4A.5.5 0 0 1 1 3h10a.5.5 0 0 1 0 1H1a.5.5 0 0 1-.5-.5z"/>
				</svg>
				Weather
			</NavLink>
		</div>
	</nav>
</div>
```

- [ ] Delete `src/Notes.Web/Components/Layout/NavMenu.razor.css`

```powershell
Remove-Item src/Notes.Web/Components/Layout/NavMenu.razor.css
```

#### Step 5 Verification Checklist

- [ ] Run `dotnet build` - no errors
- [ ] Verify NavMenu.razor.css file is deleted
- [ ] Navigate to home page
- [ ] Verify all 3 navigation icons display correctly (Home, Counter, Weather)
- [ ] Verify navigation link text appears next to icons
- [ ] Test mobile responsive behavior (resize to < 768px width)
- [ ] Verify hamburger menu toggle button appears on mobile
- [ ] Click hamburger to toggle navigation menu visibility
- [ ] Test navigation links work correctly

**Note:** Complete styling will be applied in Step 6 via app.css component classes.

#### Step 5 STOP & COMMIT

**STOP & COMMIT:** Test the changes, then stage and commit:

```powershell
git add src/Notes.Web/Components/Layout/NavMenu.razor
git add src/Notes.Web/Components/Layout/NavMenu.razor.css
git commit -m "Convert NavMenu to inline SVG icons and remove scoped CSS"
```

---

### Step 6: Extract Common Component Patterns

Create Tailwind component classes in app.css to replace the deleted scoped CSS files.

- [ ] Open `src/Notes.Web/wwwroot/css/input.css`
- [ ] Replace entire content with Tailwind import and component layer:

```css
@import "tailwindcss";

@layer components {
  /* Button styles */
  .btn-primary {
    @apply bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded transition-colors;
  }
  
  /* Layout components */
  .page {
    @apply relative flex flex-col md:flex-row;
  }
  
  .sidebar {
    background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
    @apply md:w-64 md:h-screen md:sticky md:top-0;
  }
  
  main {
    @apply flex-1;
  }
  
  .top-row {
    @apply bg-gray-100 border-b border-gray-300 flex items-center justify-end h-14;
  }
  
  .top-row a,
  .top-row .btn-link {
    @apply whitespace-nowrap ml-6 no-underline hover:underline;
  }
  
  .top-row a:first-child,
  .top-row .btn-link:first-child {
    @apply overflow-hidden text-ellipsis;
  }
  
  @media (max-width: 640px) {
    .top-row {
      @apply justify-between;
    }
    
    .top-row a,
    .top-row .btn-link {
      @apply ml-0;
    }
  }
  
  @media (min-width: 768px) {
    .top-row {
      @apply sticky top-0 z-10;
    }
    
    .top-row.auth a:first-child {
      @apply flex-1 text-right w-0;
    }
    
    .top-row {
      @apply pl-8 pr-6;
    }
    
    article {
      @apply pl-8 pr-6;
    }
  }
  
  .content {
    @apply md:pl-8 md:pr-6;
  }
  
  /* NavMenu components */
  .navbar-toggler {
    @apply appearance-none cursor-pointer w-14 h-10 text-white absolute top-2 right-4 border border-white/10 md:hidden;
    background: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='rgba%28255, 255, 255, 0.55%29' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e") no-repeat center/1.75rem rgba(255, 255, 255, 0.1);
  }
  
  .navbar-toggler:checked {
    @apply bg-white/50;
  }
  
  .navbar-brand {
    @apply text-lg;
  }
  
  .nav-item {
    @apply text-sm pb-2;
  }
  
  .nav-item:first-of-type {
    @apply pt-4;
  }
  
  .nav-item:last-of-type {
    @apply pb-4;
  }
  
  .nav-item a {
    @apply text-gray-300 rounded h-12 flex items-center leading-[3rem];
  }
  
  .nav-item a.active {
    @apply bg-white/[0.37] text-white;
  }
  
  .nav-item a:hover {
    @apply bg-white/10 text-white;
  }
  
  .nav-scrollable {
    @apply hidden;
  }
  
  .navbar-toggler:checked ~ .nav-scrollable {
    @apply block;
  }
  
  @media (min-width: 768px) {
    .nav-scrollable {
      @apply block h-[calc(100vh-3.5rem)] overflow-y-auto;
    }
  }
  
  /* Error UI */
  #blazor-error-ui {
    @apply bg-yellow-100 bottom-0 shadow-md hidden fixed left-0 px-5 py-2.5 w-full z-[1000];
  }
  
  #blazor-error-ui .dismiss {
    @apply cursor-pointer absolute right-3 top-2;
  }
}
```

- [ ] Build Tailwind CSS to generate app.css:

```powershell
cd src/Notes.Web
npm run build:css
```

- [ ] Optionally update Counter.razor to use extracted component class:

```razor
<button class="btn-primary" @onclick="IncrementCount">Click me</button>
```

#### Step 6 Verification Checklist

- [ ] Run `npm run build:css` - no errors
- [ ] Verify `wwwroot/css/app.css` is generated
- [ ] Run `dotnet build` - no errors
- [ ] Navigate to all pages and verify complete styling:
  - [ ] Home (`/`) - layout, sidebar gradient, top-row
  - [ ] Counter (`/counter`) - button styling with btn-primary class
  - [ ] Weather (`/weather`) - table styling
  - [ ] Error (`/error`) - red text
- [ ] Test responsive design:
  - [ ] Desktop (>768px): Sidebar on left, sticky, gradient visible
  - [ ] Mobile (<768px): Sidebar collapses, hamburger menu appears
- [ ] Test interactive elements:
  - [ ] Hamburger menu toggle works on mobile
  - [ ] Navigation links highlight when active (white background)
  - [ ] Navigation links show hover state (light white overlay)
  - [ ] Button hover state works (darker blue)
- [ ] Verify icons display correctly in all states
- [ ] Check browser DevTools console for errors (should be none)

#### Step 6 STOP & COMMIT

**STOP & COMMIT:** Test the changes, then stage and commit:

```powershell
git add src/Notes.Web/wwwroot/css/input.css
git add src/Notes.Web/wwwroot/css/app.css
git add src/Notes.Web/Components/Pages/Counter.razor
git commit -m "Extract Tailwind component classes to app.css"
```

---

### Step 7: Final Verification and Cleanup

Comprehensive testing across all pages and viewports.

- [ ] Rebuild entire solution:

```powershell
cd e:\github\NotesApp
dotnet clean
dotnet build
```

- [ ] Run the application via Aspire:

```powershell
dotnet run --project src/Notes.AppHost
```

- [ ] Open browser and test all pages:
  - [ ] Home (`/`) - verify layout and styling
  - [ ] Counter (`/counter`) - verify button and counter functionality
  - [ ] Weather (`/weather`) - verify table data loads and displays
  - [ ] Error (`/error`) - verify red error text

- [ ] Test responsive design at multiple breakpoints:
  - [ ] Mobile (375px width)
  - [ ] Tablet (768px width)
  - [ ] Desktop (1024px width)
  - [ ] Large desktop (1920px width)

- [ ] Verify no Bootstrap classes remain in codebase:

```powershell
# Search for common Bootstrap classes
cd src/Notes.Web
Select-String -Path "Components\**\*.razor" -Pattern "btn btn-|text-danger|table(?!-)|navbar|nav-link|container-fluid" -CaseSensitive
```

Expected result: No matches (or only matches in git history/comments)

- [ ] Verify deleted files:

```powershell
# These files should NOT exist
Test-Path src/Notes.Web/Components/Layout/MainLayout.razor.css
Test-Path src/Notes.Web/Components/Layout/NavMenu.razor.css
```

Expected result: Both return `False`

- [ ] Check CSS bundle size:

```powershell
# Before (estimate): ~180KB (with scoped CSS)
# After: Check actual size
(Get-Item src/Notes.Web/wwwroot/css/app.css).Length / 1KB
```

- [ ] Verify Tailwind compilation works:

```powershell
cd src/Notes.Web
npm run build:css
# Should complete without errors
```

#### Step 7 Verification Checklist

- [ ] All pages render correctly
- [ ] No visual regressions detected
- [ ] Responsive design works at all breakpoints
- [ ] Interactive elements function (buttons, navigation, toggle)
- [ ] Build succeeds without errors
- [ ] No Bootstrap classes found in components
- [ ] Both .razor.css files deleted
- [ ] Tailwind CSS compiles successfully
- [ ] CSS bundle size is reasonable
- [ ] No console errors in browser DevTools

#### Step 7 STOP & FINAL COMMIT

**STOP & COMMIT:** If all verification passes, create final commit:

```powershell
# Stage any remaining changes (if any)
git add -A

# Create final verification commit (if needed)
git commit -m "Final verification - Bootstrap to Tailwind conversion complete"

# View commit history
git log --oneline -7
```

Expected commits:
1. Convert Counter page button to Tailwind classes
2. Convert Error page to Tailwind text utilities
3. Convert Weather page table to Tailwind utilities
4. Remove MainLayout.razor.css scoped styles
5. Convert NavMenu to inline SVG icons and remove scoped CSS
6. Extract Tailwind component classes to app.css
7. Final verification - Bootstrap to Tailwind conversion complete (if needed)

---

## Success Criteria

✅ **All Bootstrap classes removed** from Razor components  
✅ **All scoped CSS files deleted** (MainLayout.razor.css, NavMenu.razor.css)  
✅ **Tailwind component classes** extracted to input.css  
✅ **Inline SVG icons** replace Bootstrap Icon classes  
✅ **Responsive design preserved** with Tailwind breakpoints  
✅ **No visual regressions** - application looks identical  
✅ **All interactive features work** (buttons, navigation, toggle)  
✅ **Build succeeds** with no errors  
✅ **CSS bundle optimized** with Tailwind's purge

---

## Rollback Plan

If issues occur at any step:

```powershell
# View recent commits
git log --oneline -5

# Rollback to specific commit
git reset --hard <commit-hash>

# Or rollback last commit (keep changes)
git reset --soft HEAD~1

# Or discard all changes
git checkout .
```

---

## Build Commands Reference

```powershell
# Install dependencies (first time only)
cd src/Notes.Web
npm install

# Build Tailwind CSS manually
npm run build:css

# Watch mode (auto-rebuild on file changes)
npm run watch:css

# Build .NET solution
cd e:\github\NotesApp
dotnet build

# Run via Aspire (recommended)
dotnet run --project src/Notes.AppHost

# Run Notes.Web standalone
dotnet run --project src/Notes.Web

# Clean and rebuild
dotnet clean
dotnet build
```

---

## Technology Stack

- **.NET:** 10.0
- **Blazor:** Server with Interactive Rendering
- **Tailwind CSS:** v4.1.18
- **Aspire:** Orchestration
- **Build Tool:** MSBuild + npm
- **CSS Input:** `wwwroot/css/input.css`
- **CSS Output:** `wwwroot/css/app.css` (auto-generated)

---

## Notes

- Tailwind CSS v4.1.18 uses CSS-based configuration (no tailwind.config.js needed)
- MSBuild automatically runs `npm run build:css` on every build
- All component classes use `@layer components` for proper CSS cascade
- Responsive breakpoint changed from 641px to 768px (Tailwind's `md:`)
- SVG icons use `currentColor` fill to inherit text color from parent
- `.razor.css` scoped files are no longer needed - all styles in input.css

---

**Estimated Total Time:** 45-90 minutes (including testing)

**Complexity:** Medium (straightforward conversion with thorough testing required)
