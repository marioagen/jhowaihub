<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mb-3">
                <div class="mb-3">
                    <h5 class="mb-0 fw-bold">{{ $t("auditor.title") }}</h5>
                    <p>
                        <small class="text-muted">{{ $t("auditor.subtitle") }}</small>
                    </p>
                </div>
                <div class="row">
                    <div class="col-12">
                        <div class="card">
                            <div class="card-body">
                                <p class="text-muted mb-0">{{ $t("auditor.placeholder") }}</p>
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
        name: "AuditorComponent",
    };
</script>
<style scoped>
    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }
</style>
