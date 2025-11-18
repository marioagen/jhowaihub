<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="home-container">
                <!-- Header Section -->
                <div class="text-center mt-4 mb-4">
                    <h1 class="home-title">{{ $t("home.title") }}</h1>
                    <p class="home-subtitle text-muted">{{ $t("home.subtitle") }}</p>
                </div>

                <!-- Plan Card -->
                <div class="card plan-card mb-4">
                    <div class="card-body text-center">
                        <p class="mb-1">{{ $t("home.planLabel") }}</p>
                        <h3 class="plan-name mb-1">{{ $t("home.planName") }}</h3>
                        <p class="text-muted mb-0">{{ $t("home.planThankYou") }}</p>
                    </div>
                </div>

                <!-- Quick Start Section -->
                <div class="quick-start-section">
                    <h2 class="quick-start-title text-center mb-4">{{ $t("home.quickStartTitle") }}</h2>
                    <div class="row">
                        <!-- Video Card -->
                        <div class="col-md-4 mb-3">
                            <div class="card quick-start-card h-100">
                                <div class="card-body text-center d-flex flex-column">
                                    <div class="icon-container mb-3">
                                        <LucideIcon icon="Video" :size="48" color="#0d6efd" />
                                    </div>
                                    <h5 class="card-title">{{ $t("home.platformCard.title") }}</h5>
                                    <p class="card-text text-muted flex-grow-1">
                                        {{ $t("home.platformCard.description") }}
                                    </p>
                                    <a 
                                        v-if="config.links.videoUrl" 
                                        :href="config.links.videoUrl" 
                                        target="_blank"
                                        class="btn btn-link text-decoration-none"
                                    >
                                        {{ $t("home.platformCard.button") }} →
                                    </a>
                                    <button 
                                        v-else 
                                        class="btn btn-link text-decoration-none" 
                                        disabled
                                    >
                                        {{ $t("home.platformCard.button") }} →
                                    </button>
                                </div>
                            </div>
                        </div>

                        <!-- Guide Card -->
                        <div class="col-md-4 mb-3">
                            <div class="card quick-start-card h-100">
                                <div class="card-body text-center d-flex flex-column">
                                    <div class="icon-container mb-3">
                                        <LucideIcon icon="BookOpen" :size="48" color="#0d6efd" />
                                    </div>
                                    <h5 class="card-title">{{ $t("home.workflowCard.title") }}</h5>
                                    <p class="card-text text-muted flex-grow-1">
                                        {{ $t("home.workflowCard.description") }}
                                    </p>
                                    <a 
                                        v-if="config.links.guideUrl" 
                                        :href="config.links.guideUrl" 
                                        target="_blank"
                                        class="btn btn-link text-decoration-none"
                                    >
                                        {{ $t("home.workflowCard.button") }} →
                                    </a>
                                    <button 
                                        v-else 
                                        class="btn btn-link text-decoration-none" 
                                        disabled
                                    >
                                        {{ $t("home.workflowCard.button") }} →
                                    </button>
                                </div>
                            </div>
                        </div>

                        <!-- Docs Card -->
                        <div class="col-md-4 mb-3">
                            <div class="card quick-start-card h-100">
                                <div class="card-body text-center d-flex flex-column">
                                    <div class="icon-container mb-3">
                                        <LucideIcon icon="FileText" :size="48" color="#0d6efd" />
                                    </div>
                                    <h5 class="card-title">{{ $t("home.docsCard.title") }}</h5>
                                    <p class="card-text text-muted flex-grow-1">
                                        {{ $t("home.docsCard.description") }}
                                    </p>
                                    <a 
                                        v-if="config.links.docsUrl" 
                                        :href="config.links.docsUrl" 
                                        target="_blank"
                                        class="btn btn-link text-decoration-none"
                                    >
                                        {{ $t("home.docsCard.button") }} →
                                    </a>
                                    <button 
                                        v-else 
                                        class="btn btn-link text-decoration-none" 
                                        disabled
                                    >
                                        {{ $t("home.docsCard.button") }} →
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
export default {
    name: "HomePage",
    data() {
        return {
            config: {
                links: {
                    videoUrl: "",
                    guideUrl: "",
                    docsUrl: ""
                }
            }
        };
    },
    async mounted() {
        // Load config from src/config/config.js
        try {
            const configModule = await import('@/config/config.js');
            this.config = configModule.default;
        } catch (error) {
            console.warn('Config file not found or error loading it:', error);
        }
    }
};
</script>

<style scoped>
.home-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
}

.home-title {
    font-size: 2rem;
    font-weight: bold;
    color: #212529;
    margin-bottom: 1rem;
}

.home-subtitle {
    font-size: 1rem;
    color: #6c757d;
}

.plan-card {
    background: linear-gradient(135deg, #0073e6 0%, #005bb5 100%);
    color: white;
    border: none;
    border-radius: 12px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.plan-card .card-body {
    padding: 2rem;
}

.plan-name {
    color: white;
    font-weight: bold;
    font-size: 1.5rem;
}

.quick-start-section {
    margin-top: 3rem;
}

.quick-start-title {
    font-size: 1.5rem;
    font-weight: bold;
    color: #212529;
}

.quick-start-card {
    border: 1px solid #dee2e6;
    border-radius: 12px;
    transition: all 0.3s ease;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.quick-start-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
}

.quick-start-card .card-body {
    padding: 2rem;
}

.icon-container {
    display: flex;
    justify-content: center;
    align-items: center;
}

.quick-start-card .card-title {
    font-size: 1.25rem;
    font-weight: 600;
    color: #212529;
    margin-bottom: 1rem;
}

.quick-start-card .card-text {
    font-size: 0.95rem;
    line-height: 1.5;
    margin-bottom: 1rem;
}

.quick-start-card .btn-link {
    color: #0d6efd;
    font-weight: 500;
    font-size: 1rem;
    padding: 0;
}

.quick-start-card .btn-link:hover:not(:disabled) {
    color: #0a58ca;
    text-decoration: underline !important;
}

.quick-start-card .btn-link:disabled {
    color: #6c757d;
    cursor: not-allowed;
}

.scroll-area {
    height: calc(100vh - 60px);
    overflow-y: auto;
    overflow-x: hidden;
}
</style>
