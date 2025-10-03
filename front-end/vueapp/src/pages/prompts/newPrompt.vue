<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <form>
                <div class="row align-items-center mt-3">
                    <div class="col-md-8 d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <button class="btn btn-sm p-0 me-3">
                                <LucideIcon icon="ArrowLeft" :size="17" class="me-1" />
                                <span class="fw-bold">Voltar</span>
                            </button>
                            <div>
                                <div class="fw-semibold">Novo Prompt</div>
                                <div class="text-muted small">Criar um novo prompt de IA</div>
                            </div>
                        </div>

                        <button class="btn btn-sm btn-primary" type="submit">
                            <LucideIcon icon="Save" :size="17" class="me-2" /> Salvar
                        </button>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-8">
                        <div class="card mt-3">
                            <div class="card-body">
                                <h6 class="card-title mb-3">Informações básicas</h6>
                                <div class="mb-3">
                                    <label for="inputNamePrompt" class="form-label">Nome do Prompt</label>
                                    <input type="text" class="form-control" id="inputNamePrompt" aria-describedby="" placeholder="Insira o nome do prompt">
                                </div>
                                <div class="mb-3">
                                    <label for="FormControlTextarea1" class="form-label">Descrição</label>
                                    <textarea class="form-control" id="FormControlTextarea1" rows="3"></textarea>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-8">
                        <div class="card mt-3">
                            <div class="card-body">
                                <h6 class="card-title mb-3">Conteúdo do Prompt</h6>
                                <div class="mb-3">
                                    <label for="FormControlTextarea2" class="form-label">Conteúdo do Prompt</label>
                                    <textarea class="form-control" id="FormControlTextarea2" rows="3" required></textarea>
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
    export default {
        name: "PromptComponent",
        data() {
            return {
                
            }
        },
        components: {
        },
        methods: {
            
        },
        computed: {},
        created() {
           
        },
        mounted() { },
        unmounted() { }
    }

</script>
<style scoped>
    .card
    {
        border-radius: 10px;
    }
</style>