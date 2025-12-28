# Bootstrap to Tailwind CSS Conversion

**Branch:** `wip/update-to-tailwindcss`  
**Description:** Convert all Bootstrap-style classes to Tailwind CSS utilities across Notes.Web Blazor components and remove scoped CSS files.

## Goal
Migrate the Notes.Web project from Bootstrap-style CSS classes to Tailwind CSS utilities to leverage modern utility-first styling, reduce custom CSS, and improve maintainability. The project already has Tailwind CSS v4.1.18 fully configured, so this is purely a conversion task.

## Implementation Steps

### Step 1: Convert Simple Components (Counter, Error)
**Files:** 
- src/Notes.Web/Components/Pages/Counter.razor
- src/Notes.Web/Components/Pages/Error.razor

**What:** Replace Bootstrap-style classes with Tailwind utilities in the two simplest components. Convert `btn btn-primary` to Tailwind button classes (`bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded font-medium`) and `text-danger` to `text-red-600`.

**Testing:** 
- Navigate to /counter page and verify button styling and hover states
- Trigger error page and verify red text displays correctly
- Check responsive behavior on mobile and desktop

### Step 2: Convert Weather Page Table
**Files:**
- src/Notes.Web/Components/Pages/Weather.razor

**What:** Replace table-related Bootstrap classes with Tailwind table utilities (`table-auto w-full border-collapse` and related classes for table headers/cells).

**Testing:**
- Navigate to /weather page
- Verify table renders with proper borders, spacing, and alignment
- Check responsive table behavior

### Step 3: Convert MainLayout Structure
**Files:**
- src/Notes.Web/Components/Layout/MainLayout.razor
- src/Notes.Web/Components/Layout/MainLayout.razor.css (DELETE after conversion)

**What:** Convert the 95-line scoped CSS file to Tailwind utilities in MainLayout.razor. Replace flexbox layout classes, gradient sidebar background (`bg-gradient-to-b from-[rgb(5,39,103)] to-[#3a0647]`), error UI styling, and responsive media queries with Tailwind's responsive utilities (md: breakpoint).

**Testing:**
- Verify sidebar gradient displays correctly
- Test responsive layout at mobile (<768px) and desktop widths
- Check error boundary UI styling
- Confirm main content area layout and spacing
- Delete MainLayout.razor.css after successful conversion

### Step 4: Convert NavMenu Navigation
**Files:**
- src/Notes.Web/Components/Layout/NavMenu.razor
- src/Notes.Web/Components/Layout/NavMenu.razor.css (DELETE after conversion)

**What:** Convert the 130+ line scoped CSS file containing custom hamburger menu, navigation states, and Bootstrap Icons. Replace checkbox-based toggle mechanism CSS with Tailwind `peer` utility, convert 3 Bootstrap Icons (bi-house-door-fill, bi-plus-square-fill, bi-list-nested) to inline SVG elements with Tailwind classes, and convert navigation hover/active states to Tailwind utilities.

**Testing:**
- Test hamburger menu toggle on mobile
- Verify all 3 icons display correctly
- Check navigation link hover and active states
- Test responsive behavior (mobile vs desktop navigation)
- Confirm keyboard accessibility still works
- Delete NavMenu.razor.css after successful conversion

### Step 5: Final Cleanup and Verification
**Files:**
- Verify deletion of both .razor.css files
- Check build output for unused CSS warnings
- Review generated app.css size

**What:** Run full build to ensure Tailwind compilation works correctly, verify all scoped CSS files are deleted, test all pages for visual regressions, and confirm responsive design works across all breakpoints.

**Testing:**

- Run `dotnet build` and verify no errors
- Navigate through all pages (/, /counter, /weather, /error)
- Test on multiple viewport sizes (mobile, tablet, desktop)
- Verify no visual regressions in layout, colors, spacing
- Check browser DevTools for console errors
- Compare CSS bundle size before/after

### Step 6: Extract Common Component Patterns

**Files:**

- src/Notes.Web/wwwroot/css/app.css

**What:** Extract frequently used styling patterns into reusable component classes using Tailwind's `@layer components` directive. Create component classes for common patterns like buttons, navigation links, and cards to improve maintainability while keeping Tailwind's utility-first approach.

**Testing:**

- Verify extracted component classes work across all pages
- Confirm Tailwind compilation includes custom components
- Test that component classes can be extended with additional utilities
- Build and check generated CSS output

---

## Implementation Decisions

### Icon Strategy: Inline SVG ✅

Convert Bootstrap Icons to inline SVG elements with Tailwind classes for full control and no external dependencies.

### Responsive Breakpoint: Tailwind Default (768px) ✅

Use Tailwind's standard `md:` breakpoint (768px) for responsive design, providing better mobile support and consistency with Tailwind conventions.

### Component Classes: Extract Common Patterns ✅

Use `@layer components` in app.css to extract frequently used styling patterns while maintaining utility-first approach for one-off styles.

---

## Notes

- Tailwind CSS v4.1.18 is already fully installed and configured
- Build process auto-compiles Tailwind on every build
- No setup or installation steps required
- Estimated total time: 2.5-3.5 hours
- Start with Counter.razor for quick confidence-building win
