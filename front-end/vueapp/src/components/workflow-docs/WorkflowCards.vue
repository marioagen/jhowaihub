<template>
    <div class="container mt-2">
        <div class="row">
            <div class="col-3 kanban-col">
                <div class="card flex-grow-1">
                    <div class="card-header first-steps">
                        data.title
                    </div>
                    <div class="card-body">
                        <CardComponent :dataCard="dataList"
                                       color="primary"
                                       label="labelAnalyze">
                        </CardComponent>
                    </div>
                </div>
            </div>

            <div class="col-3 kanban-col">
                <div class="card flex-grow-1">
                    <div class="card-header first-steps">
                        data.title
                    </div>
                    <div class="card-body">
                            <CardComponent :dataCard="dataList"
                                           color="warning"
                                           label="labelAdvance">
                            </CardComponent>
                    </div>
                </div>
            </div>

            <div class="col-3 kanban-col">
                <div class="card flex-grow-1">
                    <div class="card-header first-steps">
                        data.title
                    </div>
                        <div class="card-body">
                            <CardComponent :dataCard="dataList"
                                           color="danger"
                                           label="labelAdvance">
                            </CardComponent>
                        </div>
                </div>
            </div>

            <div class="col-3 kanban-col">
                <div class="card flex-grow-1">
                    <div class="card-header last-step">
                        data.title
                    </div>
                    <div class="card-body">
                        <CardComponent :dataCard="dataList"
                                       color="success"
                                       label="labelAdvance">
                        </CardComponent>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    import CardComponent from "@/components/global/CardComponent.vue";
    export default {
        name: "WorkflowCards",
        components: {
            CardComponent
        },
        data: () => ({
        }),
        methods: {
        },
            
    };
</script>
<style scoped>
    .first-steps {
        background-color: #dbe9fc;
    }

    .last-step {
        background-color: #dcfce7;
    }

    .bg-primary {
        background-color: #dbeafe !important;
        color: #2b7fff !important;
    }

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

    @media (min-width: 768px) and (max-width: 991.98px) {
        .kanban-col {
            width: 100% !important;
            display: block !important;
        }
    }

</style>
