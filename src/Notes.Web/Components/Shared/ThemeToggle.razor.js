
export function setTheme(theme) {
	// Set the theme by adding or removing the 'dark' class on the html element
	// This follows Tailwind CSS dark mode class strategy
	if (theme === 'dark') {
		document.documentElement.classList.add('dark');
		document.documentElement.classList.remove('light');
	} else {
		document.documentElement.classList.remove('dark');
		document.documentElement.classList.add('light');
	}
}