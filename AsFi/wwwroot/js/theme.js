function applyTheme(theme, primaryColor) {
    const root = document.documentElement;
    const body = document.body;

    if (theme === 'light') {
        body.classList.add('light-theme');
        root.style.setProperty('--background', '#f0f2f5');
        root.style.setProperty('--glass-text', '#1a1a2e');
        root.style.setProperty('--glass-text-light', '#4a4a6e');
        root.style.setProperty('--glass-bg', 'rgba(255,255,255,0.9)');
        root.style.setProperty('--glass-border', 'rgba(0,0,0,0.1)');
    } else {
        body.classList.remove('light-theme');
        root.style.setProperty('--background', '#0a0a1f');
        root.style.setProperty('--glass-text', '#FFFFFF');
        root.style.setProperty('--glass-text-light', 'rgba(255,255,255,0.7)');
        root.style.setProperty('--glass-bg', 'rgba(13,11,28,0.95)');
        root.style.setProperty('--glass-border', 'rgba(217,170,255,0.15)');
    }

    let primaryGrad = '';
    let accent2 = '';
    switch (primaryColor) {
        case 'red':
            primaryGrad = 'linear-gradient(135deg,#dc3545,#c82333)';
            accent2 = '#ff9999';
            break;
        case 'yellow':
            primaryGrad = 'linear-gradient(135deg,#ffc107,#fd7e14)';
            accent2 = '#ffe5b4';
            break;
        case 'green':
            primaryGrad = 'linear-gradient(135deg,#28a745,#20c997)';
            accent2 = '#a8e6cf';
            break;
        case 'blue':
            primaryGrad = 'linear-gradient(135deg,#0d6efd,#0b5ed7)';
            accent2 = '#a3c2f0';
            break;
        default:
            primaryGrad = 'linear-gradient(135deg,#7b4ae3,#922eb7)';
            accent2 = '#D9AAFFFF';
    }
    root.style.setProperty('--primary-gradient', primaryGrad);
    root.style.setProperty('--accent-2', accent2);
}

function loadUserPreferences() {
    const theme = localStorage.getItem('theme') || 'dark';
    const color = localStorage.getItem('primaryColor') || 'purple';
    applyTheme(theme, color);
}
loadUserPreferences();