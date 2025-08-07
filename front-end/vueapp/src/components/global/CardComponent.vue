<template>
        <div class="card">
            <div class="card-body">
                <p>dataCard.Title</p>
                <div class="mb-2">
                    <LucideIcon icon="FileText" size="12" class="me-1" />
                    <small>dataCard.desc</small>
                </div>
                <div class="mb-2">
                    <LucideIcon icon="Calendar" size="12" class="me-1" />
                    <small>dataCard.date</small>
                </div>
                <hr>
                <div class="mb-2">
                    <LucideIcon icon="User" size="12" class="me-1" />
                    <small>dataCard.solicitante</small>
                </div>
                <div class="mb-2">
                    <button class="btn btn-sm btn-primary" style="float:right">
                        <span>{{ $t(label) }}</span>
                        <LucideIcon icon="ChevronRight" size="16" class="me-1" />
                    </button>
                    <div :class="`badge bg-${color}`">dataCard.status</div>
                </div>
            </div>
        </div>
</template>

<script>
    export default {
        name: "CardComponent",
        props: {
            dataCard: {
                type: Object,
                required: true,
                default: () => {},
            },
            color: {
                type: String,
                required: true,
                default: "primary",
            },
            label: {
                type: String,
                required: true,
                default: "primary",
            },
        },
        data() {
        },
        methods: {
        },
    };
</script>

<style scoped>

    .bg-primary {
        background-color: #dbeafe !important;
        color: #2b7fff !important;
    }

    .bg-warning {
        background-color: #fef9c2 !important;
        color: #a65f00 !important;
    }

    .bg-danger {
        background-color: #ffedd4 !important;
        color: #ca3500 !important;
    }

    .bg-success {
        background-color: #d0fae5 !important;
        color: #007a55 !important;
    }
    .card {
        white-space: nowrap;
    }
</style>
