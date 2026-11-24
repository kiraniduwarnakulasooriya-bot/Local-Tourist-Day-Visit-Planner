let selectedIds = [];
let selectedPlacesData = [];
let map, markersGroup;

// Fallback Default Image
const defaultImg = 'https://via.placeholder.com/400x180?text=Tourist+Attraction';

// Location Coordinates (Kandy Region)
const placeCoordinates = {
    "Temple of the Sacred Tooth Relic": [7.2936, 80.6413],
    "Kandy Lake": [7.2906, 80.6412],
    "Udawattakele Forest Reserve": [7.2989, 80.6436],
    "Bahirawakanda Sri Maha Bodhi": [7.2978, 80.6300],
    "Royal Botanic Gardens Peradeniya": [7.2683, 80.5965],
    "Degaldoruwa Raja Maha Viharaya": [7.3006, 80.6583],
    "Pallekele International Cricket Stadium": [7.2800, 80.7225],
    "Embekke Devalaya": [7.1936, 80.5683],
    "Hanthana Mountain Range": [7.2561, 80.6272],
    "Victoria Dam and Reservoir Area": [7.2358, 80.7853]
};

// Initialize Leaflet Map
function initMap() {
    const mapContainer = document.getElementById('map');
    if (!mapContainer) return;

    map = L.map('map').setView([7.2906, 80.6337], 11);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    markersGroup = L.layerGroup().addTo(map);
}

// Fetch Attractions with 5-Image Carousel Slider
async function loadAttractions() {
    const searchInput = document.getElementById('searchInput');
    const categorySelect = document.getElementById('categorySelect');
    const container = document.getElementById('attractionsList');

    if (!container) return; 

    const keyword = searchInput ? searchInput.value : '';
    const categoryId = categorySelect ? categorySelect.value : '';

    const res = await fetch(`/api/attractions/search?keyword=${encodeURIComponent(keyword)}&categoryId=${categoryId}`);
    const data = await res.json();

    container.innerHTML = '';

    if (data.length === 0) {
        container.innerHTML = '<div class="alert alert-warning text-center w-100">No attractions found.</div>';
        return;
    }

    data.forEach((item, index) => {
        const carouselId = `carousel-${index}`;
        const img1 = item.image1 || defaultImg;
        const img2 = item.image2 || defaultImg;
        const img3 = item.image3 || defaultImg;
        const img4 = item.image4 || defaultImg;
        const img5 = item.image5 || defaultImg;

        container.innerHTML += `
            <div class="col-md-6 mb-4">
                <div class="card h-100 shadow-sm border-0 overflow-hidden">
                    <div id="${carouselId}" class="carousel slide" data-bs-ride="carousel">
                        <div class="carousel-indicators">
                            <button type="button" data-bs-target="#${carouselId}" data-bs-slide-to="0" class="active"></button>
                            <button type="button" data-bs-target="#${carouselId}" data-bs-slide-to="1"></button>
                            <button type="button" data-bs-target="#${carouselId}" data-bs-slide-to="2"></button>
                            <button type="button" data-bs-target="#${carouselId}" data-bs-slide-to="3"></button>
                            <button type="button" data-bs-target="#${carouselId}" data-bs-slide-to="4"></button>
                        </div>
                        <div class="carousel-inner">
                            <div class="carousel-item active">
                                <img src="${img1}" onerror="this.src='${defaultImg}'" class="d-block w-100" style="height: 180px; object-fit: cover;" alt="Image 1">
                            </div>
                            <div class="carousel-item">
                                <img src="${img2}" onerror="this.src='${defaultImg}'" class="d-block w-100" style="height: 180px; object-fit: cover;" alt="Image 2">
                            </div>
                            <div class="carousel-item">
                                <img src="${img3}" onerror="this.src='${defaultImg}'" class="d-block w-100" style="height: 180px; object-fit: cover;" alt="Image 3">
                            </div>
                            <div class="carousel-item">
                                <img src="${img4}" onerror="this.src='${defaultImg}'" class="d-block w-100" style="height: 180px; object-fit: cover;" alt="Image 4">
                            </div>
                            <div class="carousel-item">
                                <img src="${img5}" onerror="this.src='${defaultImg}'" class="d-block w-100" style="height: 180px; object-fit: cover;" alt="Image 5">
                            </div>
                        </div>
                        <button class="carousel-control-prev" type="button" data-bs-target="#${carouselId}" data-bs-slide="prev">
                            <span class="carousel-control-prev-icon"></span>
                        </button>
                        <button class="carousel-control-next" type="button" data-bs-target="#${carouselId}" data-bs-slide="next">
                            <span class="carousel-control-next-icon"></span>
                        </button>
                    </div>

                    <div class="card-body p-3 d-flex flex-column">
                        <h5 class="fw-bold text-dark">${item.name}</h5>
                        <p class="text-muted small mb-2"><span class="badge bg-secondary">${item.categoryName}</span> | Distance: ~${item.distanceKm} km</p>
                        <p class="small text-secondary flex-grow-1">${item.description}</p>
                        <div class="mt-auto">
                            <button class="btn btn-sm btn-outline-info w-100 mb-2" onclick="focusOnMap('${item.name}')">🗺️ View on Map</button>
                            <button class="btn btn-sm btn-primary w-100" onclick="addToPlan(${item.attractionId}, '${item.name}', ${item.distanceKm})">+ Add to Day Plan</button>
                        </div>
                    </div>
                </div>
            </div>`;
    });
}

function focusOnMap(name) {
    const coords = placeCoordinates[name] || [7.2906, 80.6337];
    if (markersGroup) {
        markersGroup.clearLayers();
        L.marker(coords).addTo(markersGroup)
            .bindPopup(`<b>${name}</b><br>Selected Location`)
            .openPopup();
        map.setView(coords, 14);
    }
}

function addToPlan(id, name, distance) {
    if (!selectedIds.includes(id)) {
        selectedIds.push(id);
        selectedPlacesData.push({ id, name, distance: parseFloat(distance) });

        const list = document.getElementById('selectedPlacesList');
        const emptyMsg = document.getElementById('emptyMsg');
        if (emptyMsg) emptyMsg.remove();

        if (list) {
            list.innerHTML += `<li class="list-group-item d-flex justify-content-between align-items-center shadow-sm mb-1 rounded">${name} <span class="badge bg-primary rounded-pill">~${distance} km</span></li>`;
        }

        calculateTotalDistance();
        updateMapRoute();
    }
}

function calculateTotalDistance() {
    let total = selectedPlacesData.reduce((sum, place) => sum + place.distance, 0);
    const totalElem = document.getElementById('totalDistance');
    if (totalElem) totalElem.innerText = `~${total.toFixed(1)} km`;
}

function updateMapRoute() {
    if (!markersGroup) return;
    markersGroup.clearLayers();
    let latLngs = [];

    selectedPlacesData.forEach((place, index) => {
        const coords = placeCoordinates[place.name];
        if (coords) {
            latLngs.push(coords);
            L.marker(coords).addTo(markersGroup)
                .bindPopup(`<b>${index + 1}. ${place.name}</b><br>Stop #${index + 1}`);
        }
    });

    if (latLngs.length > 1) {
        const polyline = L.polyline(latLngs, { color: 'red', weight: 4, opacity: 0.8, dashArray: '10, 10' }).addTo(markersGroup);
        map.fitBounds(polyline.getBounds(), { padding: [30, 30] });
    } else if (latLngs.length === 1) {
        map.setView(latLngs[0], 13);
    }
}

async function generateItinerary() {
    if (selectedIds.length === 0) {
        return alert('Please select at least one place!');
    }
    const res = await fetch('/api/attractions/generate-plan', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(selectedIds)
    });
    const plan = await res.json();

    selectedPlacesData = plan.map(item => ({ id: item.attractionId, name: item.name, distance: parseFloat(item.distanceKm) }));

    const list = document.getElementById('selectedPlacesList');
    if (list) {
        list.innerHTML = '';
        plan.forEach((item, index) => {
            list.innerHTML += `<li class="list-group-item d-flex justify-content-between align-items-center bg-light border-start border-success border-4 mb-1"><b>${index + 1}. ${item.name}</b> <span class="badge bg-success rounded-pill">~${item.distanceKm} km</span></li>`;
        });
    }

    calculateTotalDistance();
    updateMapRoute();
    alert('Optimal sequential travel route generated and updated on the map successfully!');
}

// ================= ADMIN PANEL FUNCTIONS =================

let allAttractionsData = []; 

async function loadAdminAttractions() {
    const tbody = document.getElementById('adminTableBody');
    if (!tbody) return; 

    const res = await fetch('/api/attractions/search');
    allAttractionsData = await res.json();

    tbody.innerHTML = '';

    allAttractionsData.forEach((item, index) => {
        tbody.innerHTML += `
            <tr>
                <td><b>${item.name}</b></td>
                <td><span class="badge bg-secondary">${item.categoryName}</span></td>
                <td>~${item.distanceKm} km</td>
                <td>
                    <!-- Index එක යවා ආරක්ෂිතව Edit Modal එක Open කිරීම -->
                    <button class="btn btn-sm btn-warning me-1" onclick="openEditModalByIndex(${index})">✏️ Edit</button>
                    <button class="btn btn-sm btn-danger" onclick="deleteAttraction(${item.attractionId})">🗑️ Delete</button>
                </td>
            </tr>`;
    });
}


function openEditModalByIndex(index) {
    const item = allAttractionsData[index];
    if (!item) return;

    document.getElementById('editId').value = item.attractionId;
    document.getElementById('editName').value = item.name;
    document.getElementById('editCategory').value = item.categoryId;
    document.getElementById('editDescription').value = item.description || '';
    document.getElementById('editDistance').value = item.distanceKm;
    document.getElementById('editHours').value = item.openingHours || '';
    document.getElementById('editImg1').value = item.image1 || '';
    document.getElementById('editImg2').value = item.image2 || '';
    document.getElementById('editImg3').value = item.image3 || '';
    document.getElementById('editImg4').value = item.image4 || '';
    document.getElementById('editImg5').value = item.image5 || '';

    var editModal = new bootstrap.Modal(document.getElementById('editModal'));
    editModal.show();
}

async function saveUpdate() {
    const updatedData = {
        attractionId: parseInt(document.getElementById('editId').value),
        name: document.getElementById('editName').value,
        categoryId: parseInt(document.getElementById('editCategory').value),
        description: document.getElementById('editDescription').value,
        distanceKm: parseFloat(document.getElementById('editDistance').value),
        openingHours: document.getElementById('editHours').value,
        image1: document.getElementById('editImg1').value,
        image2: document.getElementById('editImg2').value,
        image3: document.getElementById('editImg3').value,
        image4: document.getElementById('editImg4').value,
        image5: document.getElementById('editImg5').value
    };

    const res = await fetch('/api/attractions/update', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(updatedData)
    });

    if (res.ok) {
        alert('Location details updated successfully!');

        const modalElement = document.getElementById('editModal');
        const modalInstance = bootstrap.Modal.getInstance(modalElement);
        if (modalInstance) modalInstance.hide();

        loadAdminAttractions();
        loadAttractions(); 
    } else {
        alert('Failed to update location details.');
    }
}

window.onload = () => {
    initMap();
    loadAttractions();
    loadAdminAttractions();
};