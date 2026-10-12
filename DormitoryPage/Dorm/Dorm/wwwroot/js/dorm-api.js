/**
 * Dorm Dynamic API Bridge
 * Direct Client-Side Calls to Boarding House Management System API
 */

const API_BASE = "http://localhost:5210/api";

export async function fetchAvailableRooms(filters = {}) {
    const params = new URLSearchParams();
    if (filters.dormitoryId) params.append("dormitoryId", filters.dormitoryId);
    if (filters.roomType) params.append("roomType", filters.roomType);
    if (filters.maxRate) params.append("maxRate", filters.maxRate);

    const res = await fetch(`${API_BASE}/rooms/available?${params.toString()}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}: Failed to fetch available rooms`);
    return await res.json();
}

export async function fetchRoomDetails(roomId) {
    const res = await fetch(`${API_BASE}/rooms/${roomId}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}: Room not found`);
    return await res.json();
}

export async function submitReservation(data) {
    const res = await fetch(`${API_BASE}/reservations`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    });
    const result = await res.json();
    if (!res.ok) {
        throw new Error(result.message || "Failed to submit reservation.");
    }
    return result;
}

export async function approveReservation(id) {
    const res = await fetch(`${API_BASE}/reservations/${id}/approve`, {
        method: "PUT"
    });
    if (!res.ok) throw new Error("Failed to approve reservation.");
    return true;
}

export async function cancelReservation(id) {
    const res = await fetch(`${API_BASE}/reservations/${id}/cancel`, {
        method: "PUT"
    });
    if (!res.ok) throw new Error("Failed to cancel reservation.");
    return true;
}

export async function convertReservationToLease(id, leaseData) {
    const res = await fetch(`${API_BASE}/reservations/${id}/convert-to-lease`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(leaseData)
    });
    const result = await res.json();
    if (!res.ok) throw new Error(result.message || "Failed to convert reservation to lease.");
    return result;
}

export async function submitPayment(paymentData) {
    const res = await fetch(`${API_BASE}/payments`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(paymentData)
    });
    const result = await res.json();
    if (!res.ok) {
        throw new Error(result.message || "Payment submission failed.");
    }
    return result;
}

export async function fetchStatementOfAccount(leaseId) {
    const res = await fetch(`${API_BASE}/payments/lease/${leaseId}`);
    if (!res.ok) throw new Error("Failed to retrieve Statement of Account.");
    return await res.json();
}

export async function submitMaintenanceRequest(requestData) {
    const res = await fetch(`${API_BASE}/maintenancerequests`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(requestData)
    });
    const result = await res.json();
    if (!res.ok) throw new Error(result.message || "Failed to submit maintenance request.");
    return result;
}

export async function updateMaintenanceStatus(id, status) {
    const res = await fetch(`${API_BASE}/maintenancerequests/${id}/status`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status })
    });
    if (!res.ok) throw new Error("Failed to update maintenance status.");
    return true;
}

export async function updateRoomStatus(roomId, status) {
    const res = await fetch(`${API_BASE}/rooms/${roomId}/status`, {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status })
    });
    if (!res.ok) throw new Error("Failed to update room status.");
    return true;
}

export async function terminateLease(leaseId) {
    const res = await fetch(`${API_BASE}/leases/${leaseId}/terminate`, {
        method: "POST"
    });
    if (!res.ok) throw new Error("Failed to terminate lease.");
    return true;
}

export async function resetDatabaseSeed() {
    const res = await fetch(`${API_BASE}/seed/reset`, {
        method: "POST"
    });
    if (!res.ok) throw new Error("Failed to reset seed data.");
    return true;
}

// Global Toast Notification Helper
export function showNotification(message, type = "success") {
    let container = document.getElementById("toast-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "toast-container";
        container.className = "toast-container position-fixed bottom-0 end-0 p-3";
        container.style.zIndex = "9999";
        document.body.appendChild(container);
    }

    const toastEl = document.createElement("div");
    toastEl.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'warning' ? 'warning text-dark' : 'success'} border-0 shadow-lg`;
    toastEl.setAttribute("role", "alert");
    toastEl.setAttribute("aria-live", "assertive");
    toastEl.setAttribute("aria-atomic", "true");

    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body fw-semibold">
                <i class="bi bi-${type === 'error' ? 'exclamation-octagon' : type === 'warning' ? 'exclamation-triangle' : 'check-circle'} me-2"></i>
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    container.appendChild(toastEl);
    if (window.bootstrap) {
        const bsToast = new window.bootstrap.Toast(toastEl, { delay: 4000 });
        bsToast.show();
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    } else {
        setTimeout(() => toastEl.remove(), 4000);
    }
}
