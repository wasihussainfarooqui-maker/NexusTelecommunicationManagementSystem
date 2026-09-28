// NEXUS admin interactions: mobile sidebar and smooth scroll-reveal animations.
function toggleSidebar() {
    document.getElementById('sidebar')?.classList.toggle('open');
}

document.addEventListener('DOMContentLoaded', () => {
    const items = document.querySelectorAll('.reveal');
    const observer = new IntersectionObserver((entries, obs) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('show');
                obs.unobserve(entry.target);
            }
        });
    }, { threshold: 0.08 });
    items.forEach((item, index) => {
        item.style.transitionDelay = `${Math.min(index * 35, 180)}ms`;
        observer.observe(item);
    });
});

function toggleMenu(button) {
    const nav = document.getElementById('mainNav');
    if (!nav) return;
    const open = nav.classList.toggle('open');
    button?.setAttribute('aria-expanded', open ? 'true' : 'false');
}
