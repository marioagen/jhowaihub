<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="row">
                        <div class="col-2">
                            <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="redirectToIndex">
                                <LucideIcon icon="ArrowLeft" />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">{{ $t("flow.title") }}</h5>
                                <p><small class="text-muted">{{ $t("flow.subtitle") }}</small></p>
                            </div>
                        </div>
                    </div>
                </div>                
                <div class="col-auto ms-auto">
                    <button 
                        class="btn btn-primary btn-sm me-2"
                        @click="save"
                    >
                        <LucideIcon icon="Save" :size="15" />
                        {{ $t("flow.save") }}
                    </button>
                    <button 
                        class="btn btn-outline-primary btn-table btn-sm table-btn me-2"
                        @click="downloadJson"
                    >
                        <LucideIcon icon="Download" :size="15" />
                        {{ $t("flow.downloadJson") }}
                    </button>
                    <button 
                        class="btn btn-outline-primary btn-table btn-sm table-btn me-2"
                        @click="upload"
                    >
                        <LucideIcon icon="Upload" :size="15" />
                        {{ $t("flow.upload") }}
                    </button>
                </div>
            </div>
            <hr/>
            <div class="row">
                <div class="col">
                    <button 
                        class="btn btn-primary btn-sm me-2" 
                        data-bs-toggle="collapse" 
                        data-bs-target="#toolsCollapse" 
                        aria-expanded="false" 
                        aria-controls="toolsCollapse"
                        @click="showCollapse"
                    >
                        <LucideIcon icon="Plus" :size="15" />
                        {{ isActiveCollapse ? $t("flow.hideTools") : $t("flow.showTools") }}
                    </button>
                </div>      
            </div>
            <div class="collapse" id="toolsCollapse">
                <div class="mt-3 mb-3">
                    <div class="card mb-3">
                        <div class="card-body">
                            <ToolsListComponent />
                        </div>
                    </div>
                </div>
            </div>
            <!-- Vueflow stuff -->
        </div>
    </main>
</template>

<script>
    import ToolsListComponent from '@/components/flow/ToolsListComponent.vue';
    export default {
        name: "FlowPage",
        props: {
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
        },
        data() {
            return {
                isActiveCollapse: false,
            };
        },
        components: {
            ToolsListComponent
        },
        methods: {
            redirectToIndex() {
                if(this.isEdit) {
                    return this.$router.push({ name: "EditWorkflow" });
                }
                return this.$router.push({ name: "NewWorkflow" });
            },
            showCollapse() {
                this.isActiveCollapse = !this.isActiveCollapse;
            },
            downloadJson() {
                console.log("download")
            },
            upload() {
                console.log("upload")
            },
        },
    };
</script>