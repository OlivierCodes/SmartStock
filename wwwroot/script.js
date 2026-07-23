// script.js - shared for Razor Pages

// Utility: show toast notification
function showToast(message, type = "info") {
    const toast = document.getElementById("toast");
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    toast.style.display = "block";
    setTimeout(() => { toast.style.display = "none"; }, 3000);
}

// Utility: show modal with custom HTML
function showModal(html) {
    const modal = document.getElementById("modal");
    const body = document.getElementById("modal-body");
    body.innerHTML = html;
    modal.style.display = "block";
}
function closeModal() {
    document.getElementById("modal").style.display = "none";
}
document.getElementById("modal-close")?.addEventListener("click", closeModal);

// Authentication helpers
function getToken() { return localStorage.getItem("jwtToken"); }
function setToken(token) { localStorage.setItem("jwtToken", token); }
function clearToken() { localStorage.removeItem("jwtToken"); }

async function login(event) {
    event.preventDefault();
    const email = document.getElementById("login-email").value;
    const password = document.getElementById("login-password").value;
    try {
        const resp = await fetch("/api/Auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password })
        });
        if (!resp.ok) throw new Error("Identifiants invalides");
        const data = await resp.json();
        setToken(data.token);
        showToast("Connexion réussie", "success");
        setTimeout(() => location.href = "/Dashboard", 800);
    } catch (e) {
        console.error(e);
        showToast(e.message, "error");
    }
}

async function loadDashboard() {
    const token = getToken();
    if (!token) { location.href = "/Login"; return; }
    try {
        const resp = await fetch("/api/Dashboard/summary", {
            headers: { Authorization: `Bearer ${token}` }
        });
        if (resp.status === 401) { clearToken(); location.href = "/Login"; return; }
        if (!resp.ok) throw new Error("Erreur API");
        const data = await resp.json();
        renderKpis(data);
        renderTopProducts(data.topProducts);
        renderLowStock(data.lowStockAlerts);
    } catch (e) {
        console.error(e);
        showToast(e.message, "error");
    }
}

function renderKpis(d) {
    const grid = document.getElementById("kpi-grid");
    const items = [
        { label: "Produits actifs", value: d.totalProducts },
        { label: "Stock faible", value: d.lowStockCount },
        { label: "En rupture", value: d.outOfStockCount },
        { label: "Ventes aujourd'hui (FCFA)", value: d.salesTodayAmount.toFixed(2) },
        { label: "Nb ventes aujourd'hui", value: d.salesTodayCount },
        { label: "Ventes ce mois (FCFA)", value: d.salesMonthAmount.toFixed(2) },
        { label: "Nb ventes ce mois", value: d.salesMonthCount },
        { label: "Entrées stock aujourd'hui", value: d.entriesToday },
        { label: "Sorties stock aujourd'hui", value: d.exitsToday }
    ];
    grid.innerHTML = items.map(i => `
        <div class="kpi-card"><span class="kpi-label">${i.label}</span><span class="kpi-value">${i.value}</span></div>`).join('');
}

function renderTopProducts(list) {
    const ul = document.querySelector('#top-products .product-list');
    ul.innerHTML = list.map(p => `
        <li class="product-item"><span class="product-name">${p.name}</span><span class="product-qty">${p.soldQuantity} x</span><span class="product-rev">${p.totalRevenue.toFixed(2)} FCFA</span></li>`).join('');
}

function renderLowStock(list) {
    const ul = document.querySelector('#low-stock .stock-list');
    ul.innerHTML = list.map(a => `
        <li class="stock-item"><span class="stock-name">${a.name}</span><span class="stock-qty">${a.currentStock}/${a.minStockThreshold}</span></li>`).join('');
}

function logout() { clearToken(); location.href = "/Login"; }

document.getElementById("nav-logout")?.addEventListener("click", e => { e.preventDefault(); logout(); });
if (document.getElementById("kpi-grid")) { document.addEventListener("DOMContentLoaded", loadDashboard); }

window.showToast = showToast;
window.showModal = showModal;
window.login = login;
