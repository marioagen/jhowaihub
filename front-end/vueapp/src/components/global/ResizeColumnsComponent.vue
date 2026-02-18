<template>
    <div class="resize-columns">
        <div
            ref="resizeOverlayRef"
            class="resize-columns__overlay"
            :class="{ 'resize-columns__overlay--active': isDragging }"
            @mouseup="stopResize"
        />
        <div
            ref="splitRowRef"
            class="resize-columns__row"
            :style="{ minHeight: minHeight }"
        >
            <div
                class="resize-columns__panel resize-columns__panel--left"
                :class="{ 'resize-columns__panel--dragging': isDragging }"
                :style="{ width: leftPanelPercent + '%' }"
            >
                <slot name="left" />
            </div>
            <div
                class="resize-columns__resizer"
                @mousedown="startResize"
                :class="{ 'resize-columns__resizer--dragging': isDragging }"
                role="separator"
                :aria-valuenow="leftPanelPercent"
                :aria-valuemin="minLeftPercent"
                :aria-valuemax="maxLeftPercent"
                aria-label="Resize columns"
            />
            <div
                class="resize-columns__panel resize-columns__panel--right"
                :class="{ 'resize-columns__panel--dragging': isDragging }"
            >
                <slot name="right" />
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "ResizeColumnsComponent",
        props: {
            defaultLeftPercent: {
                type: Number,
                default: 50,
            },
            minLeftPercent: {
                type: Number,
                default: 20,
            },
            maxLeftPercent: {
                type: Number,
                default: 80,
            },
            minHeight: {
                type: String,
                default: "300px",
            },
            preferenceKey: {
                type: String,
                default: null,
            },
        },
        data() {
            const saved = this.getSavedLeftPercent();
            const initial =
                saved !== null
                    ? Math.min(this.maxLeftPercent, Math.max(this.minLeftPercent, saved))
                    : this.defaultLeftPercent;
            return {
                leftPanelPercent: initial,
                isDragging: false,
            };
        },
        watch: {
            defaultLeftPercent(value) {
                const clamped = Math.min(this.maxLeftPercent, Math.max(this.minLeftPercent, value));
                this.leftPanelPercent = clamped;
            },
            preferenceKey() {
                const saved = this.getSavedLeftPercent();
                if (saved !== null) {
                    this.leftPanelPercent = Math.min(
                        this.maxLeftPercent,
                        Math.max(this.minLeftPercent, saved)
                    );
                }
            },
        },
        methods: {
            getSavedLeftPercent() {
                if (!this.preferenceKey || !this.$store?.state?.userPreferences) {
                    return null;
                }
                const value = this.$store.state.userPreferences[this.preferenceKey];
                return typeof value === "number" ? value : null;
            },
            startResize() {
                this.isDragging = true;
                const overlay = this.$refs.resizeOverlayRef;
                if (overlay) {
                    overlay.style.pointerEvents = "auto";
                }
                document.addEventListener("mousemove", this.onResize, true);
                document.addEventListener("mouseup", this.stopResize, true);
                document.body.style.userSelect = "none";
                document.body.style.cursor = "col-resize";
            },
            onResize(e) {
                if (!this.isDragging || !this.$refs.splitRowRef) return;
                const el = this.$refs.splitRowRef;
                const rect = el.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const percent = Math.round((x / rect.width) * 100);
                const clamped = Math.min(
                    this.maxLeftPercent,
                    Math.max(this.minLeftPercent, percent)
                );
                this.leftPanelPercent = clamped;
            },
            stopResize() {
                this.isDragging = false;
                const overlay = this.$refs.resizeOverlayRef;
                if (overlay) {
                    overlay.style.pointerEvents = "";
                }
                document.removeEventListener("mousemove", this.onResize, true);
                document.removeEventListener("mouseup", this.stopResize, true);
                document.body.style.userSelect = "";
                document.body.style.cursor = "";
                if (this.preferenceKey) {
                    this.$store.commit("setUserPreference", {
                        key: this.preferenceKey,
                        value: this.leftPanelPercent,
                    });
                }
                this.$emit("update:left-percent", this.leftPanelPercent);
            },
        },
    };
</script>
<style scoped>
    .resize-columns {
        width: 100%;
    }

    .resize-columns__overlay {
        position: fixed;
        inset: 0;
        z-index: 9999;
        cursor: col-resize;
        pointer-events: none;
    }

    .resize-columns__overlay.resize-columns__overlay--active {
        pointer-events: auto;
    }

    .resize-columns__row {
        display: flex;
        align-items: stretch;
        width: 100%;
    }

    .resize-columns__panel {
        overflow: hidden;
        display: flex;
        flex-direction: column;
        min-width: 0;
    }

    .resize-columns__panel--dragging {
        pointer-events: none;
    }

    .resize-columns__panel--left {
        flex-shrink: 0;
    }

    .resize-columns__panel--right {
        flex: 1;
        min-width: 0;
    }

    .resize-columns__resizer {
        flex-shrink: 0;
        width: 24px;
        margin: 0 8px;
        cursor: col-resize;
        position: relative;
        display: flex;
        align-items: stretch;
        justify-content: center;
        background: transparent;
        transition: background 0.15s;
    }

    .resize-columns__resizer::before {
        content: "";
        position: absolute;
        left: 50%;
        top: 0;
        bottom: 0;
        width: 0;
        margin-left: -2px;
        border-left: 4px dotted var(--bs-border-color, #dee2e6);
        transition: border-color 0.15s;
    }

    .resize-columns__resizer:hover::before,
    .resize-columns__resizer.resize-columns__resizer--dragging::before {
        border-left-color: var(--bs-primary, #0d6efd);
        border-left-width: 5px;
        margin-left: -3px;
    }

    .resize-columns__resizer:hover,
    .resize-columns__resizer.resize-columns__resizer--dragging {
        background: rgba(13, 110, 253, 0.06);
    }

    .resize-columns__resizer::after {
        content: "";
        position: absolute;
        left: 50%;
        top: 0;
        bottom: 0;
        width: 28px;
        margin-left: -14px;
    }
</style>
