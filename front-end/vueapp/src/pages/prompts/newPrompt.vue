<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <form @submit="save">
                <div class="row align-items-center mt-3">
                    <div class="col-md-8 d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <button class="btn btn-sm p-0 me-3" @click="redirectToPromptList">
                                <LucideIcon icon="ArrowLeft" :size="17" class="me-1" />
                                <span class="fw-bold">{{$t('labelBack')}}</span>
                            </button>
                            <div>
                                <div class="fw-semibold">{{$t('prompts.newPrompt')}}</div>
                                <div class="text-muted small">{{$t('prompts.subtitleNew')}}</div>
                            </div>
                        </div>
                        <button class="btn btn-sm btn-primary" type="submit">
                            <LucideIcon icon="Save" :size="17" class="me-2" />{{$t('labelSave')}}
                        </button>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-8">
                        <div class="card mt-3">
                            <div class="card-body">
                                <h6 class="card-title mb-3">{{$t('prompts.information')}}</h6>
                                <div class="mb-3">
                                    <label for="inputNamePrompt" class="form-label">{{$t('prompts.namePrompt')}}</label>
                                    <input type="text" class="form-control" id="inputNamePrompt" aria-describedby="" :placeholder="$t('prompts.placeholderNamePrompt')" v-model="form.name" >
                                </div>
                                <div class="mb-3">
                                    <label for="FormControlTextarea1" class="form-label">{{$t('labelDescription')}}</label>
                                    <textarea class="form-control" id="FormControlTextarea1" v-model="form.description" rows="3"></textarea>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-8">
                        <div class="card mt-3">
                            <div class="card-body">
                                <div class="row align-items-center">
                                    <div class="col-md-12 d-flex justify-content-between align-items-center">
                                        <div class="d-flex align-items-center">
                                            <h6 class="card-title mb-3">{{$t('prompts.promptContent')}}</h6>
                                        </div>
                                        <button class="btn btn-sm p-0 me-3" @click="redirectToPromptList">
                                            <LucideIcon icon="Copy" :size="17" class="me-1" />
                                            <span class="fw-bold">{{$t('labelCopy')}}</span>
                                        </button>
                                    </div>
                                    <div class="mb-3">
                                        <label for="FormControlTextarea2" class="form-label">{{$t('prompts.promptContent')}}</label>
                                        <textarea class="form-control" id="FormControlTextarea2" v-model="form.text" rows="3"></textarea>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </form>
        </div>
   </main>
</template>
<script>
    import PromptService from "@/services/prompts/PromptsService";
    export default {
        name: "PromptComponent",
        props: {
            id: {
                type: Number,
                required: false
            }
        },
        data() {
            return {
                form: {
                    name:  '',
                    description: '',
                    text: '',
                },
                idEdit : 0,
            }
        },
        components: {
        },
        methods: {
            redirectToPromptList: function () {
                this.$router.push({ name: "Prompt" });
            },
            save: function (e) {
                if (this.idEdit !== undefined) {
                    this.updatePrompt();
                }
                else {
                    this.createPrompt();
                }
            },
            findById: function (id) {
                PromptService.getPromptById(id)
                    .then((response) => {
                        this.form = {
                            name: response.name,
                            description: response.description,
                            text: response.text,
                        }
                    });
            },
            updatePrompt: function () {
                var paramsData = {
                    id: this.idEdit,
                    name: this.form.name,
                    description: this.form.description,
                    text: this.form.text,
                };
                PromptService.updatePrompt(paramsData)
                    .then((response) => {
                        try {
                            if (!response) {
                                 return this.$notify({
                                    title: 'prompts.title',
                                    message: 'prompts.updateError',
                                    variant: 'danger',
                                    icon: 'CircleX',
                                });
                            }
                            return this.$notify({
                                title: 'prompts.title',
                                message: 'prompts.updateSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                            
                        }
                        catch (e) {
                             return this.$notify({
                                title: 'prompts.title',
                                message: 'prompts.updateError',
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        finally{
                            this.redirectToPromptList();
                        }
                    });
            },
            createPrompt: function () {
                var paramsData = {
                    name: this.form.name,
                    description: this.form.description,
                    text: this.form.text,
                };
                PromptService.createPrompt(paramsData)
                    .then((response) => {
                        try {
                            if (!response) {
                                return this.$notify({
                                    title: 'prompts.title',
                                    message: 'prompts.createError',
                                    variant: 'danger',
                                    icon: 'CircleX',
                                });
                            }
                        }
                        catch (e) {
                             return this.$notify({
                                title: 'prompts.title',
                                message: 'prompts.createErrorError',
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        finally{
                             return this.redirectToPromptList();
                        }
                    });
            }
        },
        mounted() {
            this.idEdit = this.$route.query.id
            if (this.idEdit !== undefined) {
                this.findById(this.idEdit);
            }
        },
        unmounted() { }
    }

</script>
<style scoped>
    .card
    {
        border-radius: 10px;
    }
</style>