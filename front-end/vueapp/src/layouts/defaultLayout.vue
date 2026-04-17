<template>
    <div
        class="app-layout d-flex flex-column h-100"
        style="height: 100vh"
    >
        <div
            class="d-flex flex-grow-1"
            style="overflow: hidden; position: relative; height: 100%"
        >
            <div
                :class="[
                    'sidebar-wrapper',
                    { collapsed: isSidebarCollapsed, 'sidebar-loading': !isSidebarVisible },
                ]"
            >
                <SidebarComponent
                    :key="languageChange"
                    :isCollapsed="isSidebarCollapsed"
                    :menuActive="sidebarData"
                    @toggle-collapse="toggleSidebar"
                />
            </div>

            <div :class="['content-wrapper', { collapsed: isSidebarCollapsed }]">
                <NavbarComponent
                    :key="languageChange"
                    :isSidebarCollapsed="isSidebarCollapsed"
                />
                <div class="horizontal-separator-fixed"></div>
                <router-view :key="updatePage" />
            </div>

            <div
                class="vertical-menu-separator"
                :style="{ left: isSidebarCollapsed ? '60px' : '240px' }"
            ></div>
        </div>
    </div>
</template>
<script>
    import signalRService from "@/services/signalR/signalRServices.js";
    import GlobalEventService from "@/services/globalEventService";
    import SidebarComponent from "@/components/layout/SidebarComponent.vue";
    import NavbarComponent from "@/components/layout/NavbarComponent.vue";

    const SIDEBAR_COLLAPSE_WIDTH = 768;

    export default {
        name: "DefaultLayout",
        components: {
            NavbarComponent,
            SidebarComponent,
        },
        watch: {
            "$store.state.userProfile.language": function () {
                this.languageChange++;
            },
        },
        data() {
            return {
                languageChange: 0,
                sidebarData: "",
                isSidebarCollapsed: window.innerWidth < SIDEBAR_COLLAPSE_WIDTH,
                isSidebarVisible: false,
                signalrAnonymizationReady: "AnonymizationReady",
            };
        },
        async mounted() {
            this.$store.commit("clearUploadNotifications");
            this.$nextTick(() => {
                requestAnimationFrame(() => {
                    this.isSidebarVisible = true;
                });
            });
            window.addEventListener("resize", this.checkWindowSize);
            GlobalEventService.on("uploadInProgress", this.handleUploadInProgress);
            GlobalEventService.on("uploadComplete", this.handleUploadComplete);
            GlobalEventService.on("uploadStarted", this.handleUploadStarted);

            await signalRService.startConnection();
            signalRService.on(this.signalrAnonymizationReady, (message) => {
                this.$store.commit("addAnonimyzationNotification", {
                    id: `anon-${message.documentId}`,
                    fileName: `O documento #${message.documentId} foi anonimizado com sucesso e está pronto para visualização.`,
                    link: message.url,
                });
            });
        },
        beforeUnmount() {
            window.removeEventListener("resize", this.checkWindowSize);
            GlobalEventService.off("uploadInProgress", this.handleUploadInProgress);
            GlobalEventService.off("uploadComplete", this.handleUploadComplete);
            GlobalEventService.off("uploadStarted", this.handleUploadStarted);
            signalRService.off(this.signalrAnonymizationReady);
        },
        computed: {
            updatePage() {
                return this.languageChange + this.$route.fullPath;
            },
        },
        methods: {
            checkWindowSize() {
                this.isSidebarCollapsed = window.innerWidth < SIDEBAR_COLLAPSE_WIDTH;
            },
            toggleSidebar() {
                this.isSidebarCollapsed = !this.isSidebarCollapsed;
            },
            handleUploadComplete(payload) {
                const { nameFile, success } = payload || {};
                if (!nameFile) return;
                const list = this.$store.state.uploadNotifications || [];
                const inProgress = list.find(
                    (n) => n.fileName === nameFile && n.status === "in_progress"
                );
                if (inProgress) {
                    this.$store.commit("setUploadNotificationComplete", {
                        id: inProgress.id,
                        success,
                    });
                    this.checkAllUploadsComplete();
                }
            },
            checkAllUploadsComplete() {
                const list = this.$store.state.uploadNotifications || [];
                const uploadBatch = list.filter((n) => n.id && String(n.id).startsWith("upload-"));
                if (uploadBatch.length === 0) return;
                const allCompleted = uploadBatch.every((n) => n.status === "completed");
                if (!allCompleted) return;
                const allSuccess = uploadBatch.every((n) => n.success !== false);
                const allFailed = uploadBatch.every((n) => n.success === false);
                if (allSuccess) {
                    GlobalEventService.emit("all-uploads-complete", false);
                    this.$notify({
                        title: this.$t("documents.uploadedFiles"),
                        message: this.$t("documents.uploadedFiles"),
                        variant: "success",
                        icon: "CircleCheck",
                    });
                } else if (allFailed) {
                    GlobalEventService.emit("all-uploads-complete", false);
                    this.$notify({
                        title: this.$t("documents.uploadError"),
                        message: this.$t("documents.uploadedFilesError"),
                        variant: "danger",
                        icon: "CircleX",
                    });
                } else {
                    GlobalEventService.emit("refresh-once", false);
                }
            },
            handleUploadInProgress(payload) {},
            handleUploadStarted(payload) {
                const { namesFiles } = payload || {};
                if (!namesFiles || !Array.isArray(namesFiles)) return;
                this.$store.commit("clearInProgressUploadNotifications", { namesFiles });
                const base = Date.now();
                namesFiles.forEach((name, i) => {
                    this.$store.commit("addUploadNotification", {
                        id: `upload-${base}-${i}-${name}`,
                        fileName: name,
                        status: "in_progress",
                        success: true,
                    });
                });
            },
        },
    };
</script>
<style scoped>
    .sidebar-wrapper {
        transition:
            transform 350ms cubic-bezier(0.25, 0.46, 0.45, 0.94),
            opacity 350ms cubic-bezier(0.25, 0.46, 0.45, 0.94),
            width 0.3s ease;
    }

    .sidebar-wrapper.sidebar-loading {
        transform: translateX(-100%);
        opacity: 0;
    }
</style>
