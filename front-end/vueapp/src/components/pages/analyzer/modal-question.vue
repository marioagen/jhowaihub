<template>
    <div class="modal fade show" id="exampleModalForm" tabindex="-1" aria-labelledby="exampleModalLabel">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">
                        {{ dataQuiz.title }} 
                        <span class="badge rounded-pill bg-primary badge-custom" title="Total"> 
                            <i class="fas fa-pen-square"></i> {{ dataQuiz.questions.length }}
                        </span>
                    </h5>
                    <button type="button" class="btn-close" :title="$t('labelClose')" data-bs-dismiss="modal" aria-label="Close" @click="close"></button>
                </div>
                <div class="modal-body">
                    <div style="overflow: auto; max-height: 329px;">
                        <ul class="list-group">
                            <li class="list-group-item" v-for="(item, index) in dataQuiz.questions" :key="index">
                                {{ index+1 }}. {{ item.description }}
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "ModalQuestion",
        props: {
            dataQuiz: {
                required: true,
                type: Object,
                default: {}
            },
        },
        data() {
            return {}
        },
        methods: {
            close: function () {
                this.$emit('close');
            },
        },
    }
</script>
<style scoped>
    .show {
        display: block;
        padding-right: 17px;
    }

    .prevent-text-select {
        -webkit-user-select: none; /* Safari */
        -ms-user-select: none; /* IE 10 and IE 11 */
        user-select: none; /* Standard syntax */
    }

    .btn-custom-cancel {
        font-weight: inherit !important;
        padding: 8px 12px !important;
        border: 0 !important;
    }
</style>
