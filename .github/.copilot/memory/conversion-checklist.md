# Bootstrap to Tailwind Conversion Checklist

## Quick Reference

### Components to Convert (5 files)
- [ ] `Components/Pages/Home.razor` - No classes (✓ already done)
- [ ] `Components/Pages/Counter.razor` - btn, btn-primary
- [ ] `Components/Pages/Weather.razor` - table
- [ ] `Components/Pages/Error.razor` - text-danger (2 instances)
- [ ] `Components/Layout/MainLayout.razor` - page, sidebar, top-row, px-4, content
- [ ] `Components/Layout/NavMenu.razor` - 11+ Bootstrap classes

### CSS Files to Convert (2 files)
- [ ] `Components/Layout/MainLayout.razor.css` - 95 lines
- [ ] `Components/Layout/NavMenu.razor.css` - 130+ lines

## Conversion Priority

### Phase 1: Simple Pages (Start Here)
1. **Counter.razor** - 1 button conversion
2. **Error.razor** - 2 text color conversions  
3. **Weather.razor** - 1 table conversion

### Phase 2: Layout Components
4. **MainLayout.razor** - Core layout structure
5. **MainLayout.razor.css** - Responsive layout, sidebar, error UI

### Phase 3: Complex Navigation
6. **NavMenu.razor** - Navigation structure
7. **NavMenu.razor.css** - Icons, hamburger menu, responsive

## Class Conversion Quick Map

### Buttons
```
btn btn-primary → bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded
```

### Text Colors
```
text-danger → text-red-600
```

### Tables
```
table → table-auto w-full border-collapse
```

### Spacing (Already Compatible!)
```
px-3 → px-3 (no change)
px-4 → px-4 (no change)
ps-3 → ps-3 (no change)
```

### Layout
```
container-fluid → w-full px-4
flex-column → flex flex-col
```

### Navigation
```
navbar navbar-dark → bg-gray-900 text-white
navbar-brand → text-lg font-semibold
nav → flex
nav-item → (remove, use flex child)
nav-link → block py-2 px-3 hover:bg-white/10 rounded
```

## Custom CSS Conversions

### Gradient Background
```css
/* From MainLayout.razor.css */
background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);

/* To Tailwind in MainLayout.razor */
class="bg-gradient-to-b from-[rgb(5,39,103)] to-[#3a0647]"
```

### Sticky Sidebar
```css
/* From MainLayout.razor.css */
width: 250px;
height: 100vh;
position: sticky;
top: 0;

/* To Tailwind */
class="w-[250px] h-screen sticky top-0"
```

### Responsive Flexbox
```css
/* From MainLayout.razor.css */
.page {
    flex-direction: column;
}
@media (min-width: 641px) {
    .page { flex-direction: row; }
}

/* To Tailwind */
class="flex flex-col md:flex-row"
```

## Icon Strategy

### Current Approach
- `.bi-*` classes with SVG data URIs in CSS
- 3 icons: home, plus-square, list

### Recommended Approach
**Option 1**: Create Blazor Icon Components
```razor
<HomeIcon class="w-5 h-5" />
```

**Option 2**: Inline SVG with Tailwind
```razor
<svg class="w-5 h-5 mr-3" fill="white">...</svg>
```

**Option 3**: Use Heroicons
```bash
npm install heroicons
```

## Testing Checklist

After each component conversion:
- [ ] Desktop layout (≥ 641px)
- [ ] Mobile layout (< 641px)
- [ ] Interactive states (hover, active)
- [ ] Dark theme compatibility
- [ ] Build succeeds (Tailwind compilation)

## Files to Delete After Completion

Once conversion is complete:
- [ ] `Components/Layout/MainLayout.razor.css`
- [ ] `Components/Layout/NavMenu.razor.css`

## Component-Specific Notes

### NavMenu.razor
- Hamburger menu uses checkbox toggle
- Keep `navbar-toggler` checkbox element
- Convert styling to Tailwind
- Preserve click behavior: `onclick="document.querySelector('.navbar-toggler').click()"`

### MainLayout.razor
- Error UI (`#blazor-error-ui`) needs fixed positioning
- Top row has deep selectors for child links
- Sidebar gradient is brand-specific

### Counter.razor
- Simple button, good starting point
- Test hover state after conversion

## Tailwind Config Recommendations

Consider adding to `input.css`:

```css
@import "tailwindcss";

@layer components {
  .btn-primary {
    @apply bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded transition-colors;
  }
  
  .nav-link {
    @apply block py-2 px-3 text-gray-300 hover:bg-white/10 hover:text-white rounded transition-colors;
  }
  
  .nav-link-active {
    @apply bg-white/30 text-white;
  }
}
```

## Build Verification

After changes:
```bash
dotnet build
# Or just save files - MSBuild will auto-run npm build:css
```

Check `wwwroot/css/app.css` to see new Tailwind classes.
