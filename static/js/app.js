// Update date
function updateDate() {
    const now = new Date();
    const options = { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' };
    document.getElementById('current-date').textContent = 'Date: ' + now.toLocaleDateString('fr-FR', options).replace(',', '');
}
updateDate();

// Toggle to dashboard
document.getElementById('login-btn').addEventListener('click', function() {
    document.getElementById('login-screen').classList.add('hidden');
    document.getElementById('dashboard-screen').classList.remove('hidden');
});

// Navigation tabs
const navItems = document.querySelectorAll('.nav-item');
navItems.forEach(item => {
    item.addEventListener('click', function() {
        navItems.forEach(nav => nav.classList.remove('active'));
        this.classList.add('active');
    });
});