const spanish = {
    welcomeMsg: "Bienvenido",
    Admin: "Admin",
    transferListTitle: "Lista para seleccionar",
    transferListPlaceholder: "Buscar en la lista",
    unexpectedError:
        "Ocurrió un error inesperado. Por favor, póngase en contacto con el administrador del sistema.",
    common: {
        action: "Acción",
        advance: "Avance",
        analyze: "Analizar",
        analyzed: "Analizado",
        almost: "Ya casi has llegado!",
        apply: "Aplicar",
        back: "Volver",
        cancel: "Cancelar",
        confirm: "Confirmar",
        consult: "Consultar",
        create: "Crear",
        close: "Cerrar",
        clearSelection: "Selección clara",
        copy: "Copiar texto",
        delete: "Borrar",
        description: "Descripción",
        edit: "Editar",
        edited: "Editado",
        editedFail: "Error al editar",
        expand: "Expandir",
        loading: "Cargando",
        manage: "Administrar",
        model: "Modelo",
        name: "Nombre",
        new: "Nuevo",
        notice: "Aviso",
        output: "Salida",
        order: "Orden",
        owner: "Dueño",
        processing: "Procesando",
        reprocess: "Reprocesar",
        save: "Guardar",
        send: "Enviar",
        select: "Seleccionar",
        selected: "Seleccionada",
        selectedList: "Lista seleccionada",
        actions: "Acciones",
        signOut: "Desconectar",
        status: "Estado",
        success: "Éxito",
        upload: "Subir",
        home: "Home",
        failed: "fallido!",
        caution: "Atención!",
        error: "Error",
        character: "carácter",
        characters: "caracteres",
        lines: "Líneas por página",
        showingToTotal: "del",
        notAllowed: "No permitido",
        thisActionCannotBeUndone:
            "Esta acción no se puede deshacer. ¿Está seguro de que desea eliminarlo?",
        textCopiedToClipboard:
            "Texto copiado al portapapeles.",
        id: "ID",
        all: "Todo",
        selectAll: "Seleccionar Todos",
        selectAnItemFromTheList:
            "Seleccione un elemento de la lista.",
        warning: "Atención",
    },
    modals: {
        message: "¿Está seguro?",
    },
    documents: {
        title: "Documentos",
        subtitle:
            "Gestiona documentos y extrae información",
        notFound: "No se encontraron documentos",
        removeTitle:
            "Está a punto de eliminar un documento del sistema",
        createBtn: "Nuevo documento",
        removeSuccess: "Documento eliminado con éxito",
        removeError: "Error al eliminar el documento",
        errors: {
            invalidType: "Tipo de documento no válido",
            removeTypeError:
                "Error al eliminar tipo de documento",
            uploadError: "Algo salió mal",
            uploadFailure: "Error de carga",
            uploadedFilesError:
                "¡Error al cargar archivos!",
            unselectedDocuments:
                "Ningún documento seleccionado",
        },
        createdDate: "Fecha de carga",
        teams: "Equipo",
        workflows: "Workflows",
        statusList: {
            notAnalyzed: "En espera de análisis",
        },
        actions: {
            consult: "Consultar",
        },
        upload: {
            title: "Nuevo Documento",
            subtitle:
                "Sube nuevos documentos para su análisis",
            cardTitle: "Cargar Documento",
            dropZone:
                "Arrastra y suelta tus archivos PDF abajo para comenzar o haz clic en el área para explorarlos",
            removeAllDropzone:
                "Eliminar archivos del área de carga",
            linkWorkflow: "Asociar a un flujo de trabajo",
            linkSubtitle:
                "Selecciona al menos un flujo de trabajo para asociar al documento.",
            noWorkflowFound:
                "No se encontró ningún flujo de trabajo vinculado a los equipos del usuario",
            selectionList: "Lista de selección",
            warningWorkflowNotListed:
                "¿El flujo de trabajo no aparece en la lista? Es porque tus equipos no tienen uno asociado.",
            noFileChosen: "Ningún archivo seleccionado",
            noTeamChosen: "Ningún equipo seleccionado",
        },
        workflowListModal: {
            title: "Seleccionar flujo de trabajo",
            titleMessage:
                "Este documento está asociado a varios flujos de trabajo. Elija cuál desea ver:",
            searchPlaceholder: "Buscar flujo de trabajo...",
            clickToView: "Haga clic para ver",
            nothingFound:
                "No se encontraron flujos de trabajo. Si no hay un flujo vinculado, considere reenviar el archivo y vincular un flujo de trabajo.",
            cancel: "Cancelar",
            errorToGetWorkflows:
                "Error al buscar flujos de trabajo",
            errorUnexpected: "Error inesperado.",
        },
        documentName: "Nombre del documento",
        documentType: "Tipo de documento",
        documentNameOrDescription:
            "Nombre o descripción del documento",
        documentTypeAlreadyExists:
            "Este tipo de documento ya existe",
        documentTypeSuccess:
            "Tipo de documento insertado exitosamente",
        documentTypeRemoveSuccess:
            "Tipo de documento eliminado correctamente",
        documentTypeEditSuccess:
            "Tipo de documento actualizado exitosamente",
        documentHasAlreadyBeenStandardizedPreviously:
            "El documento ya ha sido estandarizado anteriormente. <br/> Espere, será redirigido a la página de inicio.",
        documentTranscript: "Transcripción de documentos",
        ocrText: "Texto OCR",
        descriptionDocumentNote:
            "Descripción (única para varios documentos)",
        noDocumentsWereFound:
            "No se encontraron documentos",
        backToListDocuments: "Volver a los documentos",
        teamsTitleDocuments: "Asociarse con equipos",
        teamsSubtextDocuments:
            "Seleccione al menos un equipo para asociarlo al documento.",
        noTeamsFound:
            "Ningún equipo vinculado a su usuario",
        allTeams: "Todos los equipos",
        noTeams: "No hay equipos disponibles.",
        totalDocuments: "Documentos totales",
        selectedTeams: "Equipos seleccionados",
        youAreAboutToDeleteDocument:
            "Estás a punto de eliminar <br/> el documento del sistema",
        youAreAboutToDeleteDocumentQuery:
            "Estás a punto de eliminar <br/> el historial de consultas del sistema",
        searchDocument: "Buscar documento",
        generalInformation: "Cargar documento",
        historic: "Registro de respuestas",
        inclusionDate: "Fecha de inclusión",
        listing: "Listado",
        loadingTheText: "Cargando el texto",
        loadingFilePleaseWait:
            "Cargando archivo, por favor espere",
        mostRecent: "Más reciente",
        mostOlder: "Más viejo",
        pdfBack: "Volver a PDF",
        queryWithoutHistory: "Consulta sin historial",
        reduceHistory: "Reducir la historia",
        expandHistory: "Ampliar historia",
        closeHistory: "Cerrar historia",
        deleteHistory: "Borrar historial",
        viewHistory: "Ver historial",
        sendingTheDocument:
            "Enviando el documento, por favor espere.",
        preparingTheDocument: "Preparando el documento..",
        normalizingTheDocument:
            "Normalizando el documento, espera.",
        failedToNormalize: "No se pudo normalizar",
        standardizedFullText: "Texto completo normalizado",
        theFileMayBeCorrupt:
            "El archivo puede estar <br/> corrupto o tener un error. <br/> Por favor, inténtelo de nuevo.",
        theFileMayBeUnreadableOrHaveAnError:
            "El archivo puede ser <br/> ilegible o tener un error. <br/> Por favor, inténtelo de nuevo.",
        attentionDependingOnTheSpeed:
            "Atención, dependiendo de la velocidad de su conexión a Internet y del tamaño del archivo subido, esta operación puede tardar unos minutos.",
        attentionPDFDisplayFailed:
            "Se produjo un error al cargar el PDF. Haga clic aquí para intentarlo de nuevo",
        anInconsistencyWasIdentifiedInTheDocument:
            "Se identificó una inconsistencia en el documento. Vuelva a procesarlo para obtener mejores resultados.",
        uploadPdf:
            "Arrastre y suelte sus archivos PDF a continuación para empezar o haga clic en el área para navegar",
        uploadedFiles: "Todos los archivos subidos",
        uploadComplete: "Carga completa",
        showingFilesUpload: "archivos subidos",
        removeAllFilesDropzone:
            "¿Eliminar todos los archivos de la cola?",
        thisActionRemoveAllFiles:
            "Esta acción eliminará todos los archivos seleccionados para cargar",
        notReloadThePage:
            "Para evitar interrumpir el proceso de carga, no recargues la página",
        selectAFile: "Seleccione un archivo.",
        numberOfPagesHasBeenExceeded:
            "Se ha excedido el número de páginas. Comuníquese con el administrador de su plan.",
        descriptionExceeded:
            "La descripción excede el máximo de 250 caracteres.",
        closeSidebar: "Cerrar barra lateral",
        selectToDelete:
            "Seleccione al menos un documento para realizar la eliminación.",
    },
    questions: {
        title: "Preguntas",
        subtitle: "Gestiona las preguntas del sistema",
        notFound: "No hay preguntas registradas",
        filters: {
            input: "Buscar pregunta",
        },
        createBtn: "Crear pregunta",
        createSuccess: "Pregunta creada con éxito",
        createError: "Error al crear la pregunta",
        editSuccess: "Pregunta editada con éxito",
        editError: "Error al editar la pregunta",
        removeTitle:
            "Estás a punto de eliminar una pregunta del sistema",
        removeSuccess: "Pregunta eliminada con éxito",
        removeError: "Error al eliminar la pregunta",
        errorDuplicated: "Pregunta duplicada",
        createdData: "Fecha de creación",
        modalCreate: {
            title: "Crear pregunta",
            save: "Crear pregunta",
        },
        modalEdit: {
            title: "Editar pregunta",
            save: "Guardar cambios",
        },
        availableList: "Preguntas disponibles",
        questionNotFound: "Pregunta no encontrada",
        noQuestionsRegistered:
            "No hay preguntas registradas",
        noMoreQuestionsAvailable:
            "No hay más preguntas disponibles",
        noQuestionsWereFound: "No se encontraron preguntas",
        noQuestionsWereSelected:
            "No se seleccionaron preguntas.",
        editQuestion: "Editar pregunta",
        sendQuestion: "Enviar pregunta",
        selectQuestions: "Seleccionar preguntas",
        searchQuestion: "Buscar pregunta",
        viewQuestions: "Ver preguntas",
        descriptionOrQuestionId:
            "Descripción, ID o ingresa una nueva pregunta para registrarte",
        youAreAboutToDeleteQuestion:
            "Estás a punto de eliminar <br/> una pregunta del sistema",
        numberOfQuestionsHasBeenExceeded:
            "Se ha excedido el número de preguntas. Comuníquese con el administrador de su plan.",
        selectToDelete:
            "Seleccione al menos una pregunta para realizar la eliminación.",
    },
    quizzes: {
        title: "Cuestionarios",
        subtitle: "Gestiona los cuestionarios del sistema",
        notFound: "No hay cuestionarios registrados",
        createBtn: "Crear cuestionario",
        createSuccess: "Cuestionario creado con éxito",
        editSuccess: "Cuestionario actualizado con éxito",
        removeTitle:
            "Estás a punto de eliminar un cuestionario del sistema",
        removeSuccess: "Cuestionario eliminado con éxito",
        errors: {
            createError: "Error al crear cuestionario",
            editError: "Error al actualizar cuestionario",
            removeError:
                "Error al eliminar el cuestionario",
            duplicated: "Nombre del cuestionario duplicado",
            invalid: "Cuestionario no valido",
        },
        tableTitle: "Lista de cuestionarios",
        type: "Tipo de documento",
        questions: "Preguntas",
        createdDate: "Fecha de creación",
        formName: "Nombre del cuestionario",
        formSelect: "Selecciona el tipo de documento",
        formNamePlaceholder:
            "Escribe el nombre del cuestionario",
        basicInfo: "Información básica",
        basicInfoSubtitle:
            "Configura la información básica del cuestionario",
        questionsSection: {
            title: "Selección de preguntas",
            subtitle:
                "Selecciona las preguntas que formarán parte de este cuestionario",
        },
        formCreate: {
            title: "Nuevo cuestionario",
            subtitle:
                "Guarda la información del nuevo cuestionario",
        },
        formEdit: {
            title: "Edición de cuestionario",
            subtitle:
                "Actualiza la información del cuestionario",
        },
        questionnaireSearch:
            "Nombre, ID, Tipo de documento o inserte un nuevo cuestionario para registrarte",
        questionnaireAppliedSuccessfully:
            "El cuestionario ha sido aplicado correctamente, espere.",
        questionnaireAndAi: "Cuestionarios e IA",
        noQuestionnairesWereFound:
            "No se encontraron cuestionarios",
        applyQuestionnaire: "Aplicar cuestionario",
        applyingQuestionnaire: "Aplicando el cuestionario",
        applyingQuestionnaireWait:
            "Aplicando el cuestionario, por favor espere.",
        selectQuestionnaire: "Seleccione uno cuestionario",
        searchQuestionnaire: "Buscar cuestionario",
        failedToApplyQuestionnaire:
            "No se pudo aplicar el cuestionario.",
        thereIsNotEnoughCredit:
            "No hay suficiente crédito para aplicar todas las preguntas de este cuestionario.",
        youAreAboutToDeleteQuestionnaire:
            "Estás a punto de eliminar <br/> un cuestionario del sistema",
        selectToDelete:
            "Seleccione al menos un cuestionario para realizar la eliminación.",
    },
    types: {
        title: "Tipos",
        subtitle:
            "Administre los tipos de documentos del sistema",
        createBtn: "Crear tipo",
        typeDoc: "Tipo de Documento",
        typeDocAlreadyExists: "Tipo de documento duplicado",
        typeNameOrId:
            "Nombre, ID o introduce un nuevo tipo para registrarte",
        noDocumentTypeWasFound:
            "No se encontró ningún tipo de documento",
        searchTypes: "Buscar tipos",
        newType: "Crear tipo",
        youAreAboutToDeleteDocumentType:
            "Estás a punto de eliminar <br/> un tipo de documento del sistema",
        youAreAboutToDeleteType:
            "Estás a punto de eliminar un tipo del sistema",
        editTitleType: "Edición de tipo",
        saveTitleType: "Creación de tipo",
        saveType: "Crear tipo",
        createSuccess: "Tipo creado exitosamente",
        editSuccess: "Tipo actualizado exitosamente",
        removeSuccess: "Tipo eliminado exitosamente",
        errors: {
            invalid: "Tipo no valido",
            removeError: "Error al eliminar tipo",
        },
        selectToDelete:
            "Seleccione al menos un tipo para realizar la exclusión.",
    },
    management: {
        title: "Gestión de Usuarios y Equipos",
        subtitle:
            "Administra los usuarios, equipos y permisos del sistema",
        users: {
            title: "Usuarios",
            subtitle: "Administra los usuarios del sistema",
            createBtn: "Nuevo Usuario",
            createTitle: "Nuevo Usuario",
            createSubtitle:
                "Crea un nuevo usuario para el sistema",
            editTitle: "Editar Usuario",
            editSubtitle:
                "Actualiza la información del usuario seleccionado",
            saveSuccess: "Usuario guardado con éxito",
            deleteSuccess: "Usuario eliminado con éxito",
            errors: {
                invalid: "Usuario inválido",
                emailDuplicated:
                    "Correo electrónico ya registrado",
                saveError: "Error al guardar el usuario",
            },
            manageUsers: "Gestión de usuarios",
            usersMessage:
                "Administrar usuarios del sistemas",
            newUser: "Nuevo usuario",
            editUser: "Editar usuario",
            user: "Usuario",
            noUsersWasFound:
                "No se encontró ningún usuario",
            searchUsers: "Buscar usuarios...",
            typeUserName: "Introduce el nombre del usuario",
            typeUserEmail: "Introduce el email del usuario",
            newUserMessage:
                "Crear o editar un usuario en el sistema",
            newTeamUserMessage:
                "Crea un nuevo usuario que será seleccionado automáticamente",
            createTeamUser: "Crear Usuario",
            selectedUsers: "Usuarios seleccionados",
            youAreAboutToDeleteUser:
                "Estás a punto de eliminar un usuario del sistema",
            email: "Correo electrónico",
            typeEmail: "usuario{'@'}suaempresa.com",
            typeName: "Ingrese el nombre completo",
            password: "Contraseña",
            typePassword: "Ingrese la contraseña",
            confirmedPassword: "Confirmar contraseña",
            typeConfirmedPassword:
                "Ingrese la confirmación de la contraseña",
        },
        teams: {
            title: "Equipos",
            subtitle: "Administra los equipos del sistema",
            createBtn: "Nuevo Equipo",
            createTitle: "Nuevo Equipo",
            createSubtitle:
                "Crea un nuevo equipo para el sistema",
            editTitle: "Editar Equipo",
            editSubtitle:
                "Actualiza la información del equipo seleccionado",
            saveSuccess: "Equipo guardado con éxito",
            deleteSuccess: "Equipo eliminado con éxito",
            errors: {
                invalid: "Equipo inválido",
                saveError: "Error al guardar el equipo",
                deleteError: "Error al eliminar el equipo",
                duplicated: "Equipo ya registrado",
                fetchError: "Error al obtener los equipos",
                teamAlreadyExists: "Equipo ya registrado",
                deleteDocError:
                    "No se pudo eliminar el equipo: hay documentos relacionados",
            },
            teamsMessage:
                "Gestionar equipos y sus miembros",
            team: "Equipo",
            noTeamWasFound: "No se encontró ningún equipo",
            newTeam: "Nuevo equipo",
            newTeamMessage:
                "Crea un nuevo equipo en el sistema",
            typeTeamName: "Nombre del equipo",
            teamName: "Equipo",
            members: "Miembros",
            teamMembers: "Miembros del equipo",
            youAreAboutToDeleteTeam:
                "Estás a punto de eliminar un Equipo del sistema",
            searchTeams: "Buscar equipos",
        },
        profiles: {
            index: "Perfiles",
            title: "Perfiles y Permisos",
            subtitle:
                "Administra los perfiles y permisos del sistema",
            createBtn: "Nuevo Perfil",
            createTitle: "Nuevo Perfil",
            createSubtitle:
                "Crea un nuevo perfil para el sistema",
            editTitle: "Editar Perfil",
            editSubtitle:
                "Actualiza la información del perfil seleccionado",
            saveSuccess: "Perfil guardado con éxito",
            editSuccess: "Perfil actualizado con éxito",
            deleteSuccess: "Perfil eliminado con éxito",
            errors: {
                invalid: "Perfil inválido",
                saveError: "Error al guardar el perfil",
                editError: "Error al actualizar el perfil",
                deleteError: "Error al eliminar el perfil",
                removeError: "Error al eliminar perfil",
                addError: "Erro al crear perfil",
                editProfileError:
                    "Erro al actualizar perfil",
            },
            permissionsWorkflow:
                "Permisos de Flujo de Trabajo",
            profilePermissions: "Perfiles y permisos",
            newProfile: "Nuevo perfil",
            profile: "Perfil",
            profiles: "Perfiles",
            permissions: "Permisos",
            profilesMessage:
                "Administrar perfiles y permisos del sistema",
            noProfilesWereFound:
                "No se encontró ningún perfil",
            searchProfiles: "Buscar perfiles",
            typeProfileName:
                "Introduce el nombre del perfil",
            createProfile: "Crear perfil",
            editProfile: "Editar perfil",
            selectedProfiles: "Perfiles seleccionados",
            youAreAboutToDeleteProfile:
                "Estás a punto de eliminar un perfil del sistema",
            editTitleProfile: "Edición de perfil",
            editSubTitleProfile:
                "Actualizar la información del perfil en el sistema",
            saveTitleProfile: "Nuevo perfil",
            saveSubTitleProfile:
                "Crear un nuevo perfil en el sistema",
            profileAddSuccess:
                "Perfil insertado exitosamente",
            profileEditSuccess:
                "Perfil actualizado exitosamente",
            profileRemoveSuccess:
                "Perfil eliminado correctamente",
            searchPermissions: "Buscar permisos...",
            noPermissionChosen: "Ningún permiso vinculado",
        },
    },
    login: {
        index: "Login",
        title: "Iniciar sesión",
        password: "Contraseña",
        subtitle:
            "Accede a tu cuenta para gestionar documentos",
        invalid: "Campo inválido",
        error: "Error",
        loading: "Cargando...",
        sso: "Login con Microsoft",
        authSSO: "Autenticado en Microsoft",
        validateClient: "Cliente validado",
        userNotFound: "Usuario no encontrado.",
        userWithoutAccess: "Usuario sin permiso de acceso.",
        userIncorrectPassword:
            "La contraseña ingresada es incorrecta.",
        userTokenMicrosoftInvalid:
            "No se pudo validar tu autenticación. Por favor, inicia sesión nuevamente.",
        selectTenant: "Selecciona el tenant para continuar",
        continue: "Continuar",
        tenantDatabaseNotReady: "El ambiente se está preparando. Inténtelo de nuevo en unos minutos.",
        tenantNotFound: "Tenant no encontrado.",
    },
    validation: {
        required: "Campo obligatorio",
        email: "Correo electrónico inválido",
        min: "Mínimo de {limit} caracteres",
        max: "Máximo de {limit} caracteres",
        fillInThisField: "Complete este campo.",
        confirmedFieldDiffers:
            "El campo de confirmación difiere del original",
        fullname: "Ingrese el nombre y el apellido",
        password_min:
            "La contraseña debe tener al menos 6 caracteres.",
        password_lowercase:
            "La contraseña debe contener al menos una letra minúscula.",
        password_uppercase:
            "La contraseña debe contener al menos una letra mayúscula.",
        password_number:
            "La contraseña debe contener al menos un número.",
        password_special:
            "La contraseña debe contener al menos un carácter especial.",
        password_confirmed:
            "La confirmación de la contraseña no coincide.",
        hasInvalid: "Campos inválidos",
        oneStep:
            "Se requiere al menos un paso para guardar",
        oneElementArray: "Selecciona al menos una opción",
    },
    filters: {
        documentInput:
            "Buscar por documento, descripción o usuario, ...",
        workflowInput:
            "Buscar por documento, descripción o solicitante",
        toolInput: "Buscar por nombre de la herramienta",
        questionsInput: "Buscar pregunta",
        quizzesInput: "Buscar cuestionario",
        teamsSelect: {
            all: "Todos los equipos",
            none: "Selecciona un equipo",
        },
        typesSelect: {
            all: "Todos los tipos",
            none: "Selecciona un tipo",
        },
        usersSelect: {
            all: "Todos los usuarios",
            none: "Selecciona un usuario",
        },
        assignment: {
            currentUser: "Mis documentos",
            allUsers: "Todos los usuarios",
        },
        workflowSelect: {
            withWorkflow: "Todos con workflow asociado",
            none: "Selecciona un flujo de trabajo",
        },
        templates: {
            all: "Métodos HTTP",
            searchBtn: "Buscar",
        },
        sortBy: "Ordenar por",
        mostRecent: "Mas reciente",
        mostOld: "Mas antiguo",
        nameAZ: "Nombre (A-Z)",
        nameZA: "Nombre (Z-A)",
    },
    unauthorized: {
        title: "No tiene permiso para acceder a esta pantalla.",
        returnToHome: "Volver",
    },
    pagination: {
        next: "Siguiente",
        previous: "Anterior",
    },
    workflow: {
        index: "Workflow",
        title: "Tablero de Procesamiento de Documentos",
        editTitle: "Editor de Workflow",
        subtitle:
            "Visualiza el flujo de documentos a través de las etapas de procesamiento",
        subtitleEditor:
            "Gestiona y configura workflows de procesamiento de documentos",
        manage: "Gestionar workflow:",
        boardView: "Visualizando workflow:",
        steps: "Etapas del Workflow",
        error: "Error al buscar workflows",
        addBtn: "Agregar Etapa",
        addBtnDescription:
            "Haz clic para crear una nueva etapa",
        createNewStep: "Nueva Etapa",
        responsableTeam: "Equipo Responsable",
        stepTitle: "Etapa del Workflow",
        stepSubtitle: "Configura las reglas y responsables",
        managementTitle: "Gestión de Workflows",
        managementSubtitle:
            "Administra y configura workflows de procesamiento de documentos",
        access: "Acceder",
        teams: "Equipos asociados",
        actions: "Acciones",
        notFound: "No se encontró ningún workflow",
        name: "Nombre del Workflow",
        namePlaceholder: "Ex: Contract Approval",
        profiles: "Perfil Responsable",
        selectWorkflow: "Selecciona un workflow",
        selectProfile: "Selecciona un perfil",
        selectStatus: "Selecciona un estado",
        createBtn: "Nuevo Workflow",
        deleteBtn: "Eliminar",
        createSuccess: "Workflow creado con éxito",
        createError: "Error al crear workflow",
        editSuccess: "Workflow editado con éxito",
        editError: "Error al editar workflow",
        removeSuccess: "Workflow eliminado con éxito",
        removeError: "Error al eliminar workflow",
        stepFlow: "Automatización de documentos",
        nameAndAssociations: "Nombre y asociaciones",
        tools: "Herramientas",
        basicInfo: "Información Básica",
        associatedTeams: "Equipos asociados",
        stepsTitle: "Pasos del flujo de trabajo",
        stepNamePlaceholder: "Nombre del paso",
        addStep: "Agregar paso",
        addStepDescription:
            "Haga clic para crear un nuevo paso.",
        toolFlowsTitle: "Agregar flujo de herramientas",
        noStepsAvailable:
            "No hay pasos disponibles. Agregue los pasos de la fase anterior.",
        responsible: "Responsable",
        configuredTools: "Herramientas configuradas",
        addToolFlow: "Agregar flujo de herramientas",
        editToolFlow: "Editar flujo de herramientas",
        removeToolFlow: "Eliminar flujo",
        previous: "Anterior",
        next: "Próximo",
        createWorkflow: "Crear Workflow",
        saveChanges: "Guardar Cambios",
        phase1Success:
            "Información básica guardada exitosamente.",
        phase1Error: "Error al guardar información básica.",
        phase2Success: "Pasos guardados exitosamente",
        phase2Error: "Error al guardar pasos",
        phase3Success:
            "Herramientas guardadas exitosamente",
        phase3Error: "Error al guardar herramientas",
        loadError: "Error al cargar el workflow",
        formCreate: {
            title: "Crear Workflow",
            subtitle:
                "Configura las etapas del proceso de análisis de documentos",
        },
        formEdit: {
            title: "Edición de Workflow",
            subtitle:
                "Modifica las etapas y configuraciones del workflow",
        },
        labelWatchingWorkflow:
            "Visualización del workflow:",
        labelWorkflowDocs: "Workflow de documentos",
        labelWorkflowBoard:
            "Tablero de procesamiento de documentos",
        labelWorkflowSubTitle:
            "Visualice el flujo de documentos a través de los pasos de procesamiento",
        sidebar: {
            index: "Flujo de trabajo de documentos",
            management: "Gestión de flujos de trabajo",
        },
        leaveMessage: "Realizó cambios en este workflow que aún no se han guardado. Si sale ahora, se perderán todos los datos editados.",
        finalize: "Finalizar",
        saveStep: "Salvar etapa",
    },
    card: {
        userAssigned: "Responsable",
        userApplicant: "Solicitante",
        assignBtn: "Asignar",
        unassignInfo: "Desasignar documento",
        cardsOpened:
            "Hay tarjetas abiertas en este Workflow",
    },
    tools: {
        index: "Herramientas",
        title: "Herramientas",
        subtitle:
            "Administra y configura tus herramientas de procesamiento de documentos",
        notFound: "No hay herramientas registradas",
        toolNotFound: "Herramienta no encontrada",
        dependencyToolNotFound:
            "Herramienta de dependencia no encontrada",
        createBtn: "Nueva Herramienta",
        editBtn: "Guardar cambios",
        type: "Tipo",
        entry: "Entrada",
        created: "Creado en",
        createSuccess: "Herramienta creada con éxito",
        createError: "Error al crear la herramienta",
        editSuccess: "Herramienta actualizada con éxito",
        editError: "Error al actualizar la herramienta",
        removeSuccess: "Herramienta eliminada con éxito",
        removeError: "Error al eliminar la herramienta",
        removeTitle:
            "Estás a punto de eliminar una herramienta del sistema",
        dependencyRequired:
            "La herramienta de Prompt requiere al menos una dependencia",
        ocrDependencyRequired:
            "La herramienta de Prompt requiere una dependencia de una herramienta de OCR",
        form: {
            name: "Nombre de la Herramienta",
            types: "Tipo de Herramienta",
            typesSelect:
                "Selecciona el tipo de herramienta",
            entries: "Entrada",
            entriesSelect: "Selecciona el tipo de entrada",
            entriesEditable: "Entrada editable",
            outputSelect: "Selecciona el tipo de salida",
            connectorUrl: "n8n URL",
            connectorApiKey: "Clave Key",
            validatingConnector:
                "Validando la URL del conector",
            invalidConnector:
                "El conector no respondió. URL o clave no válidas.",
            validConnector: "El conector está activo",
        },
        formCreate: {
            title: "Nueva Herramienta",
            subtitle:
                "Crea una nueva herramienta para procesar documentos",
        },
        formEdit: {
            title: "Editar Herramienta",
            subtitle:
                "Actualiza la información de la herramienta seleccionada",
        },
        validationError: "Error al validar los campos",
    },
    flow: {
        title: "Flujo de Automatización",
        subtitle: "",
        downloadJson: "Descargar JSON",
        upload: "Subir",
        start: "Inicio",
        showTools: "Agregar Herramientas",
        hideTools: "Ocultar Herramientas",
        flowListEnd: "Finalizar Flujo",
        formCreate: {
            title: "Flujo de Automatización",
            subtitle:
                "Crea una nueva herramienta para procesar documentos",
        },
        formEdit: {
            title: "Flujo de Automatización:",
            subtitle:
                "Actualiza la información de la herramienta seleccionada",
        },
        sidebarTitle: "Configurar I/O:",
        sidebar: {
            filter: "Selecciona un webhook",
            inputs: "Entradas",
            dependencies: "Dependencias",
            dependenciesHint:
                "Seleccione las herramientas anteriores cuyas salidas desea usar como entrada",
            addDependency: "Agregar Dependencia",
            noDependencies:
                "No hay herramientas disponibles",
            allDependenciesSelected:
                "Todas las dependencias ya seleccionadas",
            deleteDependency: "Eliminar Dependencia",
        },
        formFlow: {
            progressFlowSuccess:
                "Flujo insertado exitosamente",
            progressFlowFail:
                "No se pudo insertar el flujo",
            progressFlowUpdateFail:
                "No se pudo actualizar el flujo. El flujo ya contiene datos de salida para las herramientas.",
            editFlowNodeSuccess:
                "Nodo editado exitosamente",
            editFlowNodeFail: "No se pudo editar el nodo",
            connectorWorkflowFail:
                "El conector del workflow no respondió. Verifica la URL y la clave API de la herramienta",
            connectorWorkflowConfigFail:
                "No se pudo recuperar la configuración del workflow. Verifica la URL y la clave API de la herramienta",
            dependenciesRequired:
                "Seleccione al menos una dependencia",
        },
    },
    prompts: {
        title: "Prompts",
        createPrompt: "Crear prompt",
        subtitle: "Gestiona los prompts del sistema",
        myPromptsBadge: "Mi prompts",
        newPrompt: "Nuevo prompt",
        namePrompt: "Nombre del prompt",
        information: "Informacion basica",
        subtitleNew: "Crear un nuevo prompt de IA",
        promptContent: "Contenido del prompt",
        placeholderNamePrompt:
            "Introduzca el nombre del prompt",
        searchPrompt: "Buscar prompt",
        labelLoadMore: "Cargar más",
        searchPrompts: "Buscar prompts",
        noPromptsListWereFound: "No se encontraron prompts",
        createSuccess: "Prompt creado exitosamente",
        createError: "No se pudieron crear el prompt",
        updateSuccess: "Prompt actualizado exitosamente",
        updateError: "No se pudieron actualizar el prompt",
        deleteSuccess: "Prompts eliminados exitosamente",
        deleteError: "No se pudieron eliminar los prompts",
        removeAllPrompts: "Eliminar prompts",
        importTitle: "Importar prompt",
        importSubtitle:
            "Selecione prompts predefinidos del sistema",
        importButton: "Importar",
        selectAllTemplates:
            "Selecionar todos los templates",
        viewComplete: "Ver completo",
        importPredefined: "Importar predefinidos",
        importError: "Error al importar prompts",
        importSuccess: "Prompts importados exitosamente",
    },
    template: {
        title: "Plantillas de API",
        tableTitle: "Plantillas",
        subtitle:
            "Administre, pruebe y edite sus plantillas de solicitud de API.",
        createTemplate: "Crear nueva plantilla",
        notFound: "No hay plantillas registradas",
        method: "Método",
        name: "Nombre",
        url: "URL",
        createBtn: "Guardar Plantilla",
        cancelBtn: "Cancelar",
        importCurl: "Importar cURL",
        requestDetails: "Detalles de la Solicitud",
        requestBody: "Cuerpo de la Solicitud",
        templateName: "Nombre de la Plantilla",
        templateNamePlaceholder:
            "ej. Procesamiento OCR de Usuario",
        endpointUrl: "URL del Endpoint",
        endpointUrlPlaceholder:
            "https://api.ejemplo.com/v1/recurso",
        queryParams: "Parámetros de Query",
        headers: "Encabezados",
        queryParameters: "Parámetros de Query",
        addParam: "Agregar Parámetro",
        noQueryParameters:
            "Sin parámetros de query. Agregue uno o escriba en la URL.",
        bodySubtitle:
            "Escriba '{' para ver las variables disponibles.",
        variablesTip:
            "Consejo: Use variables como {{ocr}} o {{prompt}} que serán reemplazadas en el momento de la ejecución.",
        formCreate: {
            title: "Crear Plantilla",
            subtitle:
                "Configure el modelo de su solicitud API.",
        },
        formEdit: {
            title: "Editar Plantilla",
            subtitle:
                "Actualice el modelo de su solicitud API.",
        },
        createSuccess: "Plantilla creada exitosamente",
        createError: "Error al crear plantilla",
        editSuccess: "Plantilla actualizada exitosamente",
        editError: "Error al actualizar plantilla",
        removeSuccess: "Plantilla eliminada exitosamente",
        removeError: "Error al eliminar plantilla",
        unselected: "Seleccione una plantilla",
        keyPlaceholder: "Key (el valor será {{nombreKey}})",
        invalidJsonFormat: "Formato JSON inválido",
    },
    analyze: {
        title: "Análisis de Documentos",
        subtitle: "Gestiona análisis de documentos",
        errorLoadDocumentData:
            "Error al cargar datos del documento",
        failedEditOutput: "No se pudo editar el output.",
        successEditOutput: "Output editada con éxito",
        extractedData: "Datos extraídos",
        askTheDoc: "Preguntar al documento",
        conversationWithDocument:
            "Escriba su pregunta sobre el documento...",
        askAI: "Preguntar a la IA",
        typeYourQuestion:
            "Escriba su pregunta sobre el documento...",
        sendQuestion: "Enviar pregunta",
        clear: "Limpiar",
        copy: "Copiar",
        previousStep: "Paso anterior",
        nextStep: "Siguiente paso",
        noDataInDocument: "No hay datos disponibles",
        failedLoadDocument:
            "No se pudo cargar el documento.",
        questionnaireToApply: "CUESTIONARIO A APLICAR",
        selectQuestionnaire:
            "Seleccione un cuestionario...",
        applyQuestionnaire: "Aplicar Cuestionario",
        questionnaireResults: "SALIDA",
        question: "Pregunta",
        answer: "Respuesta",
        errorLoadingQuestionnaires:
            "Error al cargar cuestionarios",
        errorApplyingQuestionnaire:
            "Error al aplicar cuestionario",
        successApplyingQuestionnaire:
            "Cuestionario aplicado con éxito",
        pleaseSelectQuestionnaire:
            "Por favor, seleccione un cuestionario",
        confirmed: "Confirmado",
        closeResults: "Cerrar Resultados",
        workflow: "Flujo de Trabajo",
        document: "Documento",
        askTheAi: "Pregúntale a la IA",
        findingTheBestAnswer:
            "Encontrar la mejor respuesta",
        failedNoResponse:
            "Falló, no hay respuesta del servidor.",
        failedToLoadHistory:
            "No se pudo cargar el historial.",
    },
    pages: {
        dashboard: "Panel de Control",
        management: "Gestión de Usuarios",
        documents: "Documentos",
        workflows: "Flujos de Trabajo de Documentos",
        workflowManagement: "Gestión de Flujos de Trabajo",
        types: "Tipos",
        questions: "Preguntas",
        quizzes: "Cuestionarios",
        tools: "Herramientas",
        prompts: "Prompts",
        templates: "Plantillas de API",
    },
    dashboard: {
        title: "Panel de Consumo y Ticketing",
        subtitle: "Woopi AI",
        exportBtn: "Exportar CSV",
        totalWTC: "Total WTC",
        update: "Actualizar",
        WTCText:
            "WTC (Woopi Total Cost) es la suma ponderada del consumo en el período seleccionado, basada en los multiplicadores de tu plan. No es un valor financiero.",
        filters: {
            currentMonth: "Este Mes",
            lastMonth: "Mes Pasado",
            previousSeven: "Últimos 7 Días",
            previousNinety: "Últimos 90 Días",
        },
        graphs: {
            tokenGraphTitle: "Consumo de Tokens de IA",
            tokenGraphSubtitle: "Consumo Diario de Tokens",
            pagesGraphTitle:
                "Páginas de Documentos Procesadas (OCR)",
            pagesGraphSubtitle: "Consumo Diario",
            workflowsAutomaticGraphTitle:
                "Ejecuciones de Workflows de Automatización de IA",
            workflowsGraphTitle:
                "Ejecuciones de Workflows Woopi AI",
            tokensTooltip:
                "Visualización del consumo de tokens para los diferentes modelos de Inteligencia Artificial. Los tokens son las unidades de procesamiento de texto utilizadas por los modelos.",
            pagesTooltip:
                "Cantidad de páginas procesadas mediante Reconocimiento Óptico de Caracteres (OCR), que convierte imágenes de texto en texto editable.",
            workflowAutomaticTooltip:
                "Número de veces que se ejecutaron los flujos de automatización de IA.",
            workflowTooltip:
                "Número de veces que se ejecutaron los flujos creados en la plataforma Woopi AI.",
            totalTokens: "Total de Tokens Consumidos",
            totalPages: "Total de Páginas Procesadas",
            totalWorkflowAutomatic:
                "Total de Ejecuciones Woopi AI",
            totalWorkflow:
                "Total de Ejecuciones de Automatización de IA",
            unitValue: "Valor unitario en el plan actual:",
            periodTotal: "Total del Período",
        },
        downloadCsv: "Descargar CSV",
        downloadSuccessfully: "Descargar con éxito",
        model: "Modelos",
        changeTenant: "Alternar Inquilino",
        created: "Creado em:",
    },
    plan: {
        current: "Plan Actual",
        enterprise: "Plan Enterprise",
    },
    home: {
        title: "¡Bienvenido a WOOPI AI!",
        subtitle:
            "Su viaje para automatizar y optimizar procesos con inteligencia artificial comienza ahora.",
        planLabel: "Usted ha adquirido el",
        planName: "Plan Enterprise",
        planThankYou:
            "¡Gracias por elegir nuestra plataforma!",
        quickStartTitle: "Guía de Inicio Rápido",
        platformCard: {
            title: "Conozca la Plataforma",
            description:
                "Vea un tour guiado de 5 minutos sobre las principales funcionalidades.",
            button: "Ver video",
        },
        workflowCard: {
            title: "Su Primer Workflow",
            description:
                "Siga nuestra guía paso a paso para crear su primera automatización en minutos.",
            button: "Iniciar guía",
        },
        docsCard: {
            title: "Explore la Documentación",
            description:
                "Consulte nuestra documentación completa para explorar todo el potencial de AI HUB.",
            button: "Acceder docs",
        },
    },
    permissions: {
        groups: {
            questions: "Preguntas",
            types: "Tipos",
            quizzes: "Cuestionarios",
            documents: "Documentos",
            management: "Gestión",
            users: "Usuarios",
            teams: "Equipos",
            profiles: "Perfiles",
            workflow: "Workflow",
            tools: "Herramientas",
            dashboard: "Dashboard",
            workflowStep: "Workflow Step",
            workflowmanagement: "Gestión de Flujos de Trabajo",
            prompts: "Prompts",
        },
        descriptions: {
            questions: "Ver Preguntas",
            types: "Ver Tipos",
            quizzes: "Ver Cuestionarios",
            documents: "Ver Documentos",
            management: "Ver Gestión de tablas",
            users: "Ver Usuarios",
            teams: "Ver Equipos",
            profiles: "Ver Perfiles",
            workflow: "Ver Workflow",
            tools: "Ver Herramientas",
            dashboard: "Ver Dashboard",
            workflowStepView: "Ver Pasos",
            workflowStepAccess: "Acceso Pasos",
            prompts: "Ver Prompts",
        },
    },
};

export default spanish;
